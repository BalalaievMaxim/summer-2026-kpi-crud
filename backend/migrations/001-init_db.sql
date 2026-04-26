DROP TABLE IF EXISTS Enrollment CASCADE;
DROP TABLE IF EXISTS Class CASCADE;
DROP TABLE IF EXISTS Invoice CASCADE;
DROP TABLE IF EXISTS Membership CASCADE;
DROP TABLE IF EXISTS Client CASCADE;
DROP TABLE IF EXISTS Coach CASCADE;
DROP TABLE IF EXISTS ClassType CASCADE;
DROP TABLE IF EXISTS MembershipPlan CASCADE;

CREATE TABLE MembershipPlan (
                                plan_id SERIAL PRIMARY KEY,
                                name VARCHAR(100) NOT NULL UNIQUE,
                                access VARCHAR(100) NOT NULL,
                                duration_months INTEGER NOT NULL CHECK (duration_months > 0),
                                price NUMERIC(10,2) NOT NULL CHECK (price > 0),
                                CONSTRAINT check_access_type CHECK (access IN ('gym_only', 'gym_pool', 'gym_cardio', 'all_inclusive'))
);

CREATE TABLE Client (
                        client_id SERIAL PRIMARY KEY,
                        name VARCHAR(100) NOT NULL,
                        email VARCHAR(100) NOT NULL UNIQUE,
                        password VARCHAR(255) NOT NULL,
                        phone VARCHAR(20) NOT NULL UNIQUE,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        CONSTRAINT check_email_format CHECK (email LIKE '%@%')
);

CREATE TABLE Membership (
                            membership_id SERIAL PRIMARY KEY,
                            client_id INTEGER NOT NULL,
                            plan_id INTEGER NOT NULL,
                            start_date DATE NOT NULL,
                            end_date DATE NOT NULL,
                            price NUMERIC(10,2) NOT NULL CHECK (price > 0),
                            is_active BOOLEAN DEFAULT TRUE,
                            FOREIGN KEY (client_id) REFERENCES Client(client_id) ON DELETE CASCADE,
                            FOREIGN KEY (plan_id) REFERENCES MembershipPlan(plan_id) ON DELETE RESTRICT,
                            CONSTRAINT check_dates CHECK (end_date > start_date)
);

CREATE TABLE Invoice (
                         invoice_id SERIAL PRIMARY KEY,
                         client_id INTEGER NOT NULL,
                         amount NUMERIC(10,2) NOT NULL CHECK (amount > 0),
                         date DATE NOT NULL DEFAULT CURRENT_DATE,
                         status VARCHAR(20) DEFAULT 'pending',
                         payment_method VARCHAR(50),
                         notes TEXT,
                         FOREIGN KEY (client_id) REFERENCES Client(client_id) ON DELETE CASCADE,
                         CONSTRAINT check_status CHECK (status IN ('pending', 'paid', 'overdue', 'cancelled')),
                         CONSTRAINT check_payment_method CHECK (payment_method IN ('cash', 'card', 'bank_transfer', 'online'))
);

CREATE TABLE Coach (
                       coach_id SERIAL PRIMARY KEY,
                       name VARCHAR(100) NOT NULL,
                       specialization VARCHAR(100) NOT NULL,
                       email VARCHAR(100) NOT NULL UNIQUE,
                       password VARCHAR(255) NOT NULL,
                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       CONSTRAINT check_coach_email CHECK (email LIKE '%@%')
);

CREATE TABLE ClassType (
                           class_type_id SERIAL PRIMARY KEY,
                           name VARCHAR(100) NOT NULL UNIQUE,
                           description TEXT
);

CREATE TABLE Class (
                       class_id SERIAL PRIMARY KEY,
                       class_type_id INTEGER NOT NULL,
                       coach_id INTEGER NOT NULL,
                       start_time TIMESTAMP NOT NULL,
                       end_time TIMESTAMP NOT NULL,
                       capacity INTEGER NOT NULL CHECK (capacity > 0 AND capacity <= 10),
                       current_enrollment INTEGER DEFAULT 0 CHECK (current_enrollment >= 0),
                       FOREIGN KEY (class_type_id) REFERENCES ClassType(class_type_id) ON DELETE RESTRICT,
                       FOREIGN KEY (coach_id) REFERENCES Coach(coach_id) ON DELETE RESTRICT,
                       CONSTRAINT check_class_time CHECK (end_time > start_time),
                       CONSTRAINT check_capacity_limit CHECK (current_enrollment <= capacity)
);

CREATE TABLE Enrollment (
                            enrollment_id SERIAL PRIMARY KEY,
                            client_id INTEGER NOT NULL,
                            class_id INTEGER NOT NULL,
                            registration_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (client_id) REFERENCES Client(client_id) ON DELETE CASCADE,
                            FOREIGN KEY (class_id) REFERENCES Class(class_id) ON DELETE CASCADE,
                            UNIQUE(client_id, class_id)
);

INSERT INTO MembershipPlan (name, access, duration_months, price) VALUES
                                                                      ('Basic Gym', 'gym_only', 1, 500.00),
                                                                      ('Gym + Pool', 'gym_pool', 3, 1350.00),
                                                                      ('Premium Cardio', 'gym_cardio', 6, 2400.00),
                                                                      ('VIP All Inclusive', 'all_inclusive', 12, 5400.00);

INSERT INTO Client (name, email, password, phone) VALUES
                                                      ('Олександр Петренко', 'oleksandr.p@gmail.com', 'hashed_pass_1', '+380501234567'),
                                                      ('Марія Коваленко', 'maria.k@gmail.com', 'hashed_pass_2', '+380502345678'),
                                                      ('Іван Шевченко', 'ivan.sh@gmail.com', 'hashed_pass_3', '+380503456789'),
                                                      ('Анна Бойко', 'anna.b@gmail.com', 'hashed_pass_4', '+380504567890'),
                                                      ('Дмитро Мельник', 'dmytro.m@gmail.com', 'hashed_pass_5', '+380505678901');

INSERT INTO Membership (client_id, plan_id, start_date, end_date, price, is_active) VALUES
                                                                                        (1, 1, '2024-10-01', '2024-11-01', 500.00, TRUE),
                                                                                        (2, 3, '2024-09-15', '2025-03-15', 2400.00, TRUE),
                                                                                        (3, 2, '2024-10-10', '2025-01-10', 1350.00, TRUE),
                                                                                        (4, 4, '2024-08-01', '2025-08-01', 5400.00, TRUE),
                                                                                        (5, 1, '2024-10-20', '2024-11-20', 500.00, TRUE);

INSERT INTO Invoice (client_id, amount, date, status, payment_method, notes) VALUES
                                                                                 (1, 500.00, '2024-10-01', 'paid', 'card', 'Basic membership payment'),
                                                                                 (2, 2400.00, '2024-09-15', 'paid', 'bank_transfer', 'Premium 6-month membership'),
                                                                                 (3, 1350.00, '2024-10-10', 'paid', 'cash', 'Gym + Pool 3-month plan'),
                                                                                 (4, 5400.00, '2024-08-01', 'paid', 'card', 'VIP yearly membership'),
                                                                                 (5, 500.00, '2024-10-20', 'pending', 'online', 'Awaiting payment confirmation');

INSERT INTO Coach (name, specialization, email, password) VALUES
                                                              ('Андрій Коваль', 'Yoga & Pilates', 'andriy.koval@gym.com', 'coach_pass_1'),
                                                              ('Оксана Литвин', 'Cardio & Fitness', 'oksana.l@gym.com', 'coach_pass_2'),
                                                              ('Сергій Бондар', 'Boxing & Martial Arts', 'sergiy.b@gym.com', 'coach_pass_3'),
                                                              ('Наталія Савченко', 'Swimming & Aqua Aerobics', 'natalia.s@gym.com', 'coach_pass_4');

INSERT INTO ClassType (name, description) VALUES
                                              ('Yoga', 'Relaxing yoga session for flexibility and mindfulness'),
                                              ('Boxing', 'High-intensity boxing training for strength and endurance'),
                                              ('Cardio Dance', 'Fun cardio workout with dance moves'),
                                              ('Swimming', 'Professional swimming lessons and techniques'),
                                              ('HIIT', 'High-Intensity Interval Training for maximum calorie burn');

INSERT INTO Class (class_type_id, coach_id, start_time, end_time, capacity, current_enrollment) VALUES
                                                                                                    (1, 1, '2024-11-01 09:00:00', '2024-11-01 10:00:00', 10, 3),
                                                                                                    (2, 3, '2024-11-01 10:30:00', '2024-11-01 11:30:00', 8, 2),
                                                                                                    (3, 2, '2024-11-01 17:00:00', '2024-11-01 18:00:00', 10, 4),
                                                                                                    (4, 4, '2024-11-02 08:00:00', '2024-11-02 09:00:00', 6, 1),
                                                                                                    (5, 2, '2024-11-02 18:00:00', '2024-11-02 19:00:00', 10, 3),
                                                                                                    (1, 1, '2024-11-03 09:00:00', '2024-11-03 10:00:00', 10, 2),
                                                                                                    (2, 3, '2024-11-03 16:00:00', '2024-11-03 17:00:00', 8, 1),
                                                                                                    (3, 2, '2024-11-04 17:00:00', '2024-11-04 18:00:00', 10, 2);

INSERT INTO Enrollment (client_id, class_id) VALUES
                                                 (1, 1),
                                                 (2, 1),
                                                 (3, 1),
                                                 (2, 2),
                                                 (4, 2),
                                                 (1, 3),
                                                 (2, 3),
                                                 (3, 3),
                                                 (5, 3),
                                                 (4, 4);
