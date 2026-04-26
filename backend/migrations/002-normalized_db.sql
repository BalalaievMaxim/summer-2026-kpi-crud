DROP TABLE IF EXISTS Enrollment CASCADE;
DROP TABLE IF EXISTS Class CASCADE;
DROP TABLE IF EXISTS ClassType CASCADE;
DROP TABLE IF EXISTS Invoice CASCADE;
DROP TABLE IF EXISTS Membership CASCADE;
DROP TABLE IF EXISTS PlanAccess CASCADE;
DROP TABLE IF EXISTS FacilityZone CASCADE;
DROP TABLE IF EXISTS MembershipPlan CASCADE;
DROP TABLE IF EXISTS Coach CASCADE;
DROP TABLE IF EXISTS Client CASCADE;


CREATE TABLE FacilityZone (
    zone_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE MembershipPlan (
    plan_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    duration_months INTEGER NOT NULL CHECK (duration_months > 0),
    price NUMERIC(10,2) NOT NULL CHECK (price > 0)
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

-- 1NF: таблиця many-to-many для Планів та Зон
CREATE TABLE PlanAccess (
    plan_id INTEGER NOT NULL,
    zone_id INTEGER NOT NULL,
    PRIMARY KEY (plan_id, zone_id),
    FOREIGN KEY (plan_id) REFERENCES MembershipPlan(plan_id) ON DELETE CASCADE,
    FOREIGN KEY (zone_id) REFERENCES FacilityZone(zone_id) ON DELETE CASCADE
);

-- 3NF: видалено стовпець price
CREATE TABLE Membership (
    membership_id SERIAL PRIMARY KEY,
    client_id INTEGER NOT NULL,
    plan_id INTEGER NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
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

-- 3NF: видалено current_enrollment
CREATE TABLE Class (
    class_id SERIAL PRIMARY KEY,
    class_type_id INTEGER NOT NULL,
    coach_id INTEGER NOT NULL,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NOT NULL,
    capacity INTEGER NOT NULL CHECK (capacity > 0 AND capacity <= 50),
    FOREIGN KEY (class_type_id) REFERENCES ClassType(class_type_id) ON DELETE RESTRICT,
    FOREIGN KEY (coach_id) REFERENCES Coach(coach_id) ON DELETE RESTRICT,
    CONSTRAINT check_class_time CHECK (end_time > start_time)
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