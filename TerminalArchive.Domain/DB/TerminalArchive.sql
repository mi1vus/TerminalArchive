CREATE SCHEMA IF NOT EXISTS `terminal_archive` ;

DROP TABLE IF EXISTS `terminal_archive`.`history`;
DROP TABLE IF EXISTS `terminal_archive`.`users`;
DROP TABLE IF EXISTS `terminal_archive`.`role_rights`;
DROP TABLE IF EXISTS `terminal_archive`.`rights`;
DROP TABLE IF EXISTS `terminal_archive`.`roles`;
DROP TABLE IF EXISTS `terminal_archive`.`order_details`;
DROP TABLE IF EXISTS `terminal_archive`.`details`;
DROP TABLE IF EXISTS `terminal_archive`.`orders`;
DROP TABLE IF EXISTS `terminal_archive`.`order_states`;
DROP TABLE IF EXISTS `terminal_archive`.`order_payment_types`;
DROP TABLE IF EXISTS `terminal_archive`.`order_fuels`;
DROP TABLE IF EXISTS `terminal_archive`.`terminal_parameters`;
DROP TABLE IF EXISTS `terminal_archive`.`parameters`;
DROP TABLE IF EXISTS `terminal_archive`.`terminal_groups`;
DROP TABLE IF EXISTS `terminal_archive`.`terminals`;
DROP TABLE IF EXISTS `terminal_archive`.`groups`;

CREATE TABLE IF NOT EXISTS `terminal_archive`.`groups` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`terminals` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_hasp` VARCHAR(50) NOT NULL,
  `address` VARCHAR(100) NOT NULL,
  `name` VARCHAR(50) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_hasp_UNIQUE` (`id_hasp` ASC)
);  

CREATE TABLE IF NOT EXISTS `terminal_archive`.`terminal_groups` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_terminal` INT NOT NULL,
  `id_group` INT NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `terminal_groups_UNIQUE` (`id_terminal` ASC,`id_group` ASC),
  FOREIGN KEY (`id_terminal`) REFERENCES `terminal_archive`.`terminals`(`id`),
  FOREIGN KEY (`id_group`) REFERENCES `terminal_archive`.`groups`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`parameters` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `path` VARCHAR(150) NOT NULL,
  `name` VARCHAR(150) NOT NULL,
  `description` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`terminal_parameters` (
  `id_terminal` INT NOT NULL,
  `id_parameter` INT NOT NULL,
  `value` VARCHAR(150) NOT NULL,
  `save_date` TIMESTAMP NOT NULL DEFAULT '1970-01-02 00:00:00',
  `last_edit_date` TIMESTAMP ON UPDATE CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_terminal`,`id_parameter`),
  FOREIGN KEY (`id_terminal`) REFERENCES `terminal_archive`.`terminals`(`id`),
  FOREIGN KEY (`id_parameter`) REFERENCES `terminal_archive`.`parameters`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`order_fuels` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`order_payment_types` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`order_states` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`orders` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_terminal` INT NOT NULL,
  `RNN` VARCHAR(20) NOT NULL,
  `id_fuel` INT NOT NULL,
  `id_pump` INT NOT NULL,
  `id_payment` INT NOT NULL,
  `id_state` INT NOT NULL,
  `pre_price` DECIMAL(5,2) UNSIGNED NOT NULL,
  `price` DECIMAL(5,2) UNSIGNED NOT NULL,
  `pre_quantity` DECIMAL(5,2) UNSIGNED NOT NULL,
  `quantity` DECIMAL(5,2) UNSIGNED NOT NULL,
  `pre_summ` DECIMAL(5,2) UNSIGNED NOT NULL,
  `summ` DECIMAL(5,2) UNSIGNED NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `terminal_RNN_UNIQUE` (`id_terminal` ASC,`RNN` ASC),
  FOREIGN KEY (`id_terminal`) REFERENCES `terminal_archive`.`terminals`(`id`),
  FOREIGN KEY (`id_fuel`) REFERENCES `terminal_archive`.`order_fuels`(`id`),
  FOREIGN KEY (`id_payment`) REFERENCES `terminal_archive`.`order_payment_types`(`id`),
  FOREIGN KEY (`id_state`) REFERENCES `terminal_archive`.`order_states`(`id`)
);  

CREATE TABLE IF NOT EXISTS `terminal_archive`.`details` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `description` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`order_details` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_order` INT NOT NULL,
  `id_detail` INT NOT NULL,
  `value` VARCHAR(150) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `order_detail_UNIQUE` (`id_order` ASC,`id_detail` ASC),
  FOREIGN KEY (`id_order`) REFERENCES `terminal_archive`.`orders`(`id`),
  FOREIGN KEY (`id_detail`) REFERENCES `terminal_archive`.`details`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`roles` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_group` INT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`id_group`) REFERENCES `terminal_archive`.`groups`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`rights` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(150) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`role_rights` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_role` INT NOT NULL,
  `id_right` INT NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `role_rights_UNIQUE` (`id_role` ASC,`id_right` ASC),
  FOREIGN KEY (`id_role`) REFERENCES `terminal_archive`.`roles`(`id`),
  FOREIGN KEY (`id_right`) REFERENCES `terminal_archive`.`rights`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`users` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_role` INT NOT NULL,
  `name` VARCHAR(150) NOT NULL,
  `pass` VARCHAR(32) NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `users_name_UNIQUE` (`name`  ASC),
  FOREIGN KEY (`id_role`) REFERENCES `terminal_archive`.`roles`(`id`)
);

CREATE TABLE IF NOT EXISTS `terminal_archive`.`history` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `date` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `id_terminal` INT NOT NULL,
  `id_order` INT,
  `id_state` INT NOT NULL,
  `trace` TEXT,
  `msg` TEXT NOT NULL,
  `delete` BOOLEAN NOT NULL DEFAULT false,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`id_terminal`) REFERENCES `terminal_archive`.`terminals`(`id`),
  FOREIGN KEY (`id_order`) REFERENCES `terminal_archive`.`orders`(`id`),
  FOREIGN KEY (`id_state`) REFERENCES `terminal_archive`.`order_states`(`id`)
);

INSERT INTO `terminal_archive`.`groups` (`id`,`name`) VALUES 
('1', 'Тестовая'),
('2', 'Тестовая2'),
('3', 'Объединение 1 и 2');

INSERT INTO `terminal_archive`.`terminals` (`id`,`id_hasp`, `address`, `name`) VALUES 
('1', '306061827', 'Исследователей, 15', 'Тестовый1'),
('2', '306061828', 'Онуфриева, 55', 'Тестовый2'),
('3', '306061829', 'Волгоградская, 50', 'Тестовый3'),
('4', '306061830', 'Рябинина, 19', 'Тестовый4');

INSERT INTO `terminal_archive`.`terminal_groups` (`id`,`id_terminal`,`id_group`) VALUES 
('1', '1', '1'),
('2', '2', '1'),
('3', '3', '2'),
('4', '4', '2'),
('5', '1', '3'),
('6', '2', '3'),
('7', '3', '3');

INSERT INTO `terminal_archive`.`parameters` (`id`, `path`, `name`, `description`) VALUES 
('1', 'ASU_Z_Driver', 'tid', 'идентификатор терминала'),
('2', 'ASU_Z_Driver', 'server', 'ip сервера'),
('3', 'ASU_Z_Driver', 'pumps', 'доступные колонки'),
('4', 'ASU_Z_Driver', 'card_add_zero', 'включено/выключено');


INSERT INTO `terminal_archive`.`terminal_parameters` (`id_terminal`, `id_parameter`, `value`) VALUES 
('1', '1', 'Терминал 2'),
('1', '2', '192.168.0.100');

INSERT INTO `terminal_archive`.`order_fuels` (`id`,`name`) VALUES 
('1', '-'),
('2', '95'),
('3', '98'),
('4', '80'),
('5', '-'),
('6', '92');

INSERT INTO `terminal_archive`.`order_payment_types` (`id`,`name`) VALUES 
('1', 'наличные'), 
('2', 'карты'), 
('3', 'топливные карты');

INSERT INTO `terminal_archive`.`order_states` (`id`,`name`) VALUES 
('1', 'создан'),
('2', 'готов к оплате'),
('3', 'оплачен'),
('4', 'установлен на ТРК'),
('5', 'выполнен'),
('6', 'выполнен и пересчитан'),
('1001', 'критическая ошибка'),
('1002', 'серьезная ошибка'),
('1003', 'ошибка'),
('1004', 'предупреждение');

INSERT INTO `terminal_archive`.`orders` (`id`, `id_terminal`, `RNN`, `id_fuel`, `id_pump`, `id_payment`, `id_state`, `pre_price`, `price`, `pre_quantity`, `quantity`, `pre_summ`, `summ`) VALUES 
('1', '1', '2', '1', '2', '3', '1', '4.00', '4.00', '2.00', '2.00', '8.00', '8.00'),
('2', '1', '00000120180406132211', '6', '1', '1', '1', '34.30', '33.27', '4.51', '4.51', '150.00', '150.00'),
('6', '2', '00000120180406171232', '2', '1', '1', '1', '35.70', '34.63', '17.33', '17.33', '600.00', '600.00'),
('7', '1', '00000120180406172542', '6', '1', '1', '1', '34.30', '33.27', '15.03', '15.03', '500.00', '500.00'),
('8', '2', '00000120180406174001', '6', '1', '1', '1', '33.27', '33.27', '4.51', '1.97', '150.00', '67.57'),
('9', '4', '00000220180409171112', '6', '2', '1', '1', '34.30', '34.30', '0.00', '0.00', '0.00', '0.00'),
('11', '3', '00000120180409175310', '4', '1', '1', '6', '9.70', '9.70', '61.86', '3.44', '600.00', '34.40'),
('12', '3', '00000420180409180200', '3', '4', '1', '6', '35.80', '35.80', '14.40', '0.00', '500.00', '0.00'),
('13', '3', '00000120180409180244', '4', '1', '1', '2', '0.00', '0.00', '0.00', '0.00', '0.00', '0.00');

INSERT INTO `terminal_archive`.`details` (`id`,`description`) VALUES 
('1', 'Кассир'),
('2', 'Смена');

INSERT INTO `terminal_archive`.`order_details` (`id`, `id_order`, `id_detail`, `value`) VALUES 
('1', '13', '1', '2'),
('2', '13', '2', '4');

INSERT INTO `terminal_archive`.`rights` (`id`,`name`) VALUES 
('1', 'None'),
('2', 'Read'),
('3', 'Write');

INSERT INTO `terminal_archive`.`roles` ( `id`, `id_group`, `name`) VALUES 
('1', null, 'Админы'),
('2', '1', 'Читатели 1 группы'),
('3', '2', 'Редакторы 2 группы'),
('4', null, 'Читатели'),
('5', null, 'Забаненные'),
('6', '3', 'Читатели 1 и 2 группы');

INSERT INTO `terminal_archive`.`role_rights` (`id`, `id_role`, `id_right`) VALUES 
('1', '1', '2'),
('2', '1', '3'),
('3', '2', '2'),
('4', '3', '2'),
('5', '3', '3'),
('6', '4', '2'),
('7', '5', '1'),
('8', '6', '2');

INSERT INTO `terminal_archive`.`users` (`id`, `id_role`, `name`, `pass`) VALUES 
('1', '1', 'Admin', '81dc9bdb52d04dc20036dbd8313ed055'),
('2', '3', 'Max', '81dc9bdb52d04dc20036dbd8313ed055'),
('3', '4', 'Lisa', '81dc9bdb52d04dc20036dbd8313ed055'),
('4', '5', 'John', '81dc9bdb52d04dc20036dbd8313ed055'),
('5', '6', 'Doug', '81dc9bdb52d04dc20036dbd8313ed055');

INSERT INTO `terminal_archive`.`history` (`id`, `date`, `id_terminal`, `id_order`, `id_state`, `trace`, `msg`) VALUES
('1', '2018-05-18 18:37:37', '1', '13', '2', NULL, 'заказ изменен'),
('2', '2018-05-18 18:48:02', '1', NULL, '1002', NULL, 'заказ изменен');


SELECT t.`id`, t.`id_hasp`,  g.`name` AS `группа` ,  t.`address` , t.`name`, 
p.id AS `id параметра`, p.name AS `имя параметра`, p.path AS `путь параметра` ,tp.value AS `значение параметра`, 
tp.last_edit_date, tp.save_date
FROM terminal_archive.terminals AS t
LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal
LEFT JOIN terminal_archive.groups AS g ON tg.id_group = g.id
LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
/*WHERE tp.save_date < tp.last_edit_date*/
ORDER BY t.id desc;

SELECT o.`id`, s.name AS `состояние`,  t.`name` AS `терминал` ,  `RNN` , d.description AS `доп. параметр`, od.value AS `значение`,
f.`name` AS `топливо` , p.`name` AS `оплата` , o.id_pump AS `колонка`,  
`pre_price` ,  `price` ,  `pre_quantity` ,  `quantity` ,  `pre_summ` ,  `summ` FROM terminal_archive.orders AS o
LEFT JOIN terminal_archive.order_fuels AS f ON o.id_fuel = f.id
LEFT JOIN terminal_archive.order_payment_types AS p ON o.id_payment = p.id
LEFT JOIN terminal_archive.terminals AS t ON o.id_terminal = t.id
LEFT JOIN terminal_archive.order_states AS s ON o.id_state = s.id
LEFT JOIN terminal_archive.order_details AS od ON o.id = od.id_order
LEFT JOIN terminal_archive.details AS d ON od.id_detail = d.id
/*WHERE t.id = 1*/
ORDER BY o.id desc
LIMIT 20;

SELECT t.`id`, t.`id_hasp`,  g.`name` AS `группа` ,  t.`address` , t.`name`, 
p.id AS `id параметра`, p.name AS `имя параметра` ,p.path AS `путь параметра`, tp.value AS `значение параметра`, 
tp.last_edit_date, tp.save_date
FROM terminal_archive.terminals AS t
LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal
LEFT JOIN terminal_archive.groups AS g ON tg.id_group = g.id
LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
/*WHERE tp.save_date < tp.last_edit_date AND t.hasp_id='1'*/
ORDER BY t.id asc; 
