using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using bd2.Application.Abstractions;

namespace bd2.Infrastructure.Repositories;

public class GenericRepository<T>(IDbConnection connection) : IGenericRepository<T>
    where T : class, new()
{
    private readonly string _tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;

    public T? GetById(int id)
    {
        T? entity = null;
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            entity = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                int ordinal;
                try
                {
                    ordinal = reader.GetOrdinal(prop.Name);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (!reader.IsDBNull(ordinal))
                {
                    object value = reader.GetValue(ordinal);
                    prop.SetValue(entity, value);
                }
            }
        }

        connection.Close();
        return entity;
    }

    public IEnumerable<T> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];

        var entities = new List<T>();

        using var command = connection.CreateCommand();

        var paramNames = ids.Select((_, index) => $"@id{index}").ToArray();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Id IN ({string.Join(", ", paramNames)})";

        for (int i = 0; i < ids.Length; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = paramNames[i];
            parameter.Value = ids[i];
            command.Parameters.Add(parameter);
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            T entity = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                int ordinal;
                try
                {
                    ordinal = reader.GetOrdinal(prop.Name);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (!reader.IsDBNull(ordinal))
                {
                    object value = reader.GetValue(ordinal);
                    prop.SetValue(entity, value);
                }
            }

            entities.Add(entity);
        }

        connection.Close();
        return entities;
    }

    public IEnumerable<T> GetAll()
    {
        var list = new List<T>();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName}";

        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            T entity = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                int ordinal;
                try
                {
                    ordinal = reader.GetOrdinal(prop.Name);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (!reader.IsDBNull(ordinal))
                {
                    object value = reader.GetValue(ordinal);
                    prop.SetValue(entity, value);
                }
            }

            list.Add(entity);
        }

        connection.Close();
        return list;
    }

    public void Create(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var paramNames = string.Join(", ", properties.Select(p => "@" + p.Name));

        using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames})";

        foreach (var prop in properties)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@" + prop.Name;
            parameter.Value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        command.ExecuteNonQuery();
        connection.Close();
    }

    public void Update(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new Exception("Отсутствует свойство Id у сущности");

        var idValue = idProperty.GetValue(entity);
        var properties = typeof(T).GetProperties()
            .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            .Where(p => p.GetValue(entity) != null)
            .ToArray();

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        using var command = connection.CreateCommand();
        command.CommandText = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

        foreach (var prop in properties)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@" + prop.Name;
            parameter.Value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = idValue;
        command.Parameters.Add(idParam);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        command.ExecuteNonQuery();
        connection.Close();
    }

    public void Delete(int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE Id = @Id";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        command.ExecuteNonQuery();
        connection.Close();
    }

    public void ExecuteCommand(string commandText, Dictionary<string, object>? parameters = null)
    {
        using var command = connection.CreateCommand();
        command.CommandText = commandText;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value;
                command.Parameters.Add(parameter);
            }
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        command.ExecuteNonQuery();
        connection.Close();
    }

    public IEnumerable<T> ExecuteQuery<T>(string commandText, Dictionary<string, object>? parameters = null)
        where T : class, new()
    {
        var list = new List<T>();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value;
                command.Parameters.Add(parameter);
            }
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            T entity = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                int ordinal;
                try
                {
                    ordinal = reader.GetOrdinal(prop.Name);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (!reader.IsDBNull(ordinal))
                {
                    object value = reader.GetValue(ordinal);
                    prop.SetValue(entity, value);
                }
            }

            list.Add(entity);
        }

        connection.Close();
        return list;
    }

    public TScalar ExecuteScalar<TScalar>(string commandText, Dictionary<string, object>? parameters = null)
    {
        using var command = connection.CreateCommand();
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value;
                command.Parameters.Add(parameter);
            }
        }
        var result = command.ExecuteScalar();
        return result == DBNull.Value ? default : (TScalar)Convert.ChangeType(result, typeof(TScalar));
    }
}