DO $$
DECLARE
    start_author_id INT;
    start_worker_id INT;
    start_origin_id INT;
    start_staging_id INT;
    start_inventory_id INT;
    start_hall_id INT;
    start_performance_id INT;
    start_role_id INT;
BEGIN    
    INSERT INTO Authors (AuthorName) VALUES
                                         ('Уильям Шекспир'),
                                         ('Антон Чехов'),
                                         ('Вольфганг Амадей Моцарт'),
                                         ('Петр Чайковский'),
                                         ('Габриэль Гарсиа Маркес'),
                                         ('Лев Толстой');
    
    SELECT MIN(id)-1 FROM Authors INTO start_author_id;
    
    INSERT INTO AuthorsSpecialties (AuthorId, SpecialtyName) VALUES
                                                                 (start_author_id+3, 'Композитор'),
                                                                 (start_author_id+4, 'Композитор'),
                                                                 (start_author_id+5, 'Писатель'),
                                                                 (start_author_id+6, 'Писатель'),
                                                                 (start_author_id+1, 'Писатель'),
                                                                 (start_author_id+2, 'Писатель'),
                                                                 (start_author_id+2, 'Композитор'),
                                                                 (start_author_id+1, 'Композитор');
    
    INSERT INTO Inventory (InventoryName, TotalAmount) VALUES
                                                           ('Декорации "Лес"', 10),
                                                           ('Костюмы "Эпоха"', 20),
                                                           ('Музыкальные инструменты', 50),
                                                           ('Световое оборудование', 30),
                                                           ('Мебель "Классика"', 15);

    SELECT MIN(id)-1 FROM Inventory INTO start_inventory_id;
    
    INSERT INTO Workers (Name, Specialty) VALUES
                                              ('Иван Петров', 'Режиссер'),
                                              ('Мария Сидорова', 'Композитор'),
                                              ('Алексей Кузнецов', 'Актер'),
                                              ('Елена Смирнова', 'Актер'),
                                              ('Сергей Иванов', 'Светотехник'),
                                              ('Ольга Васильева', 'Декоратор'),
                                              ('Петр Смирнов', 'Актер');

    SELECT MIN(id)-1 FROM Workers INTO start_worker_id;

    INSERT INTO Artists (Id, Grade) VALUES
                                              (start_worker_id+3, 'Ведущий'),
                                              (start_worker_id+4, 'Первый план'),
                                              (start_worker_id+7, 'Стажер');
    

    INSERT INTO Origins (OriginName, OriginComposerId, WriterId) VALUES
                                                                     ('Гамлет', start_author_id+1, start_author_id+1),
                                                                     ('Вишневый сад', start_author_id+2, start_author_id+2),
                                                                     ('Свадьба Фигаро', start_author_id+3, start_author_id+5),
                                                                     ('Евгений Онегин', start_author_id+4, start_author_id+6),
                                                                     ('Сто лет одиночества', start_author_id+3, start_author_id+5);

    SELECT MIN(id)-1 FROM Origins INTO start_origin_id;

    INSERT INTO Staging (Duration, DirectorId, StagingComposerId, OriginId) VALUES
                                                                                ('02:30:00', start_worker_id+1, start_worker_id+2, start_origin_id+1),
                                                                                ('02:00:00', start_worker_id+1, start_worker_id+2, start_origin_id+2),
                                                                                ('03:00:00', start_worker_id+1, start_worker_id+2, start_origin_id+3),
                                                                                ('02:45:00', start_worker_id+1, start_worker_id+2, start_origin_id+4),
                                                                                ('02:15:00', start_worker_id+1, start_worker_id+2, start_origin_id+5);
    
    SELECT MIN(id)-1 FROM Staging INTO start_staging_id;
    
    INSERT INTO NeededInventory (InventoryId, StagingId, Count) VALUES
                                                                    (start_inventory_id+1, start_staging_id+1, 5),
                                                                    (start_inventory_id+2, start_staging_id+1, 10),
                                                                    (start_inventory_id+3, start_staging_id+3, 20),
                                                                    (start_inventory_id+4, start_staging_id+5, 8),
                                                                    (start_inventory_id+5, start_staging_id+2, 7);
    
    INSERT INTO Halls (Capacity) VALUES
                                     (500),
                                     (800),
                                     (300),
                                     (1000);

    SELECT MIN(id)-1 FROM Halls INTO start_hall_id;
    
    INSERT INTO Roles (StagingId, RoleName) VALUES
                                                (start_staging_id+1, 'Гамлет'),
                                                (start_staging_id+1, 'Офелия'),
                                                (start_staging_id+2, 'Раневская'),
                                                (start_staging_id+2, 'Лопахин'),
                                                (start_staging_id+4, 'Онегин'),
                                                (start_staging_id+4, 'Татьяна');

    SELECT MIN(id)-1 FROM Roles INTO start_role_id;


    INSERT INTO Performances (StartDateTime, StagingId, SoldTicketsCount, HallId) VALUES
                                                                                      ('2023-11-20 19:00:00', start_staging_id+1, 450, start_hall_id+1),
                                                                                      ('2023-11-21 18:30:00', start_staging_id+2, 700, start_hall_id+2),
                                                                                      ('2023-11-22 20:00:00', start_staging_id+3, 250, start_hall_id+3),
                                                                                      ('2023-11-23 19:30:00', start_staging_id+4, 900, start_hall_id+4),
                                                                                      ('2023-11-24 18:00:00', start_staging_id+5, 100, start_hall_id+3);

    SELECT MIN(id)-1 FROM Performances INTO start_performance_id;

    INSERT INTO ArtistsInPerformances (RoleId, ArtistId, PerformanceId) VALUES
                                                                            (start_role_id+1, start_worker_id+3, start_performance_id+1),
                                                                            (start_role_id+2, start_worker_id+4, start_performance_id+1),
                                                                            (start_role_id+3, start_worker_id+4, start_performance_id+2),
                                                                            (start_role_id+4, start_worker_id+3, start_performance_id+2),
                                                                            (start_role_id+5, start_worker_id+4, start_performance_id+4),
                                                                            (start_role_id+6, start_worker_id+3, start_performance_id+4);

    INSERT INTO Users (login, hashedpassword, role) VALUES 
                                                        ('bosslogin', 'LPz/KOfLo6qjfBR52a3iBOCJSlsiJRSKBINH+B8bAkI=', 'director'), --bosspass
                                                        ('managerlogin', '9qMBtnvxdpqRSGLxvSss+GZG8YkmlFH3fwSE+krjQpM=', 'manager'), --manpass
                                                        ('warehouseboss', 'ECNUiNnouXYyud6JNZGhjE5i7UHQk0FPGsE24vj7tvo=', 'storekeeper'); --storepass
    
    END$$;
