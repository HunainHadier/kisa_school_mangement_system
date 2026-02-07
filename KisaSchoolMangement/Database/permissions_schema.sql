-- Role & permission schema additions
CREATE TABLE IF NOT EXISTS permissions (
  id INT NOT NULL AUTO_INCREMENT,
  module VARCHAR(100) NOT NULL,
  permission_key VARCHAR(100) NOT NULL,
  name VARCHAR(150) NOT NULL,
  description VARCHAR(255) DEFAULT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_permission_key (permission_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS role_permissions (
  role_id INT NOT NULL,
  permission_id INT NOT NULL,
  updated_by INT DEFAULT NULL,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (role_id, permission_id),
  KEY idx_permission_id (permission_id),
  CONSTRAINT role_permissions_ibfk_1 FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
  CONSTRAINT role_permissions_ibfk_2 FOREIGN KEY (permission_id) REFERENCES permissions (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO permissions (module, permission_key, name, description)
VALUES
  ('Students', 'students.view', 'View Students', 'Access student list and details'),
  ('Students', 'students.create', 'Create Students', 'Add new students'),
  ('Students', 'students.edit', 'Edit Students', 'Update student records'),
  ('Students', 'students.delete', 'Delete Students', 'Remove student records'),
  ('Donors', 'donors.view', 'View Donors', 'Access donor list and details'),
  ('Donors', 'donors.create', 'Create Donors', 'Add new donors'),
  ('Donors', 'donors.edit', 'Edit Donors', 'Update donor records'),
  ('Donors', 'donors.delete', 'Delete Donors', 'Remove donor records'),
  ('Exams', 'exams.view', 'View Exams', 'Access exam setup'),
  ('Exams', 'exams.manage', 'Manage Exams', 'Create and update exam setup'),
  ('Fees', 'fees.view', 'View Fees', 'Access fee structures and payments'),
  ('Fees', 'fees.manage', 'Manage Fees', 'Create and update fee structures'),
  ('Attendance', 'attendance.view', 'View Attendance', 'Access attendance data'),
  ('Attendance', 'attendance.manage', 'Manage Attendance', 'Record attendance'),
  ('Teachers', 'teachers.view', 'View Teachers', 'Access teacher profiles'),
  ('Teachers', 'teachers.manage', 'Manage Teachers', 'Create and update teacher profiles'),
  ('Users', 'users.view', 'View Users', 'Access user list'),
  ('Users', 'users.manage', 'Manage Users', 'Create and update users'),
  ('Roles', 'roles.manage', 'Manage Roles', 'Create roles and assign permissions')
ON DUPLICATE KEY UPDATE
  module = VALUES(module),
  name = VALUES(name),
  description = VALUES(description);
