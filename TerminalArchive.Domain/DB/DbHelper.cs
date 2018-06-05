using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.Domain.DB
{
    public static class DbHelper
    {
        // строка подключения к БД
        private static readonly string ConnStr /*= "server=localhost;user=MYSQL;database=terminal_archive;password=tt2QeYy2pcjNyBm6AENp;"*/;
        //private static string _connStrTest = "server=localhost;user=MYSQL;database=products;password=tt2QeYy2pcjNyBm6AENp;";

        public static Dictionary<int, Terminal> Terminals = new Dictionary<int, Terminal>();
        public static List<Group> Groups = new List<Group>();

        static DbHelper()
        {
            var rootWebConfig =
            System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MyWebSiteRoot");
            if (rootWebConfig.AppSettings.Settings.Count <= 0) return;
            var customSetting =
                rootWebConfig.AppSettings.Settings["connStr"];
            if (customSetting != null)
                ConnStr = customSetting.Value;
        }

        public static bool IsAuthorizeUser(string name, string password, MySqlConnection contextConn = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var users = 0;
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                var sql = "";
                using (var md5Hash = MD5.Create())
                {
                    var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var sBuilder = new StringBuilder();
                    foreach (var t in data)
                    {
                        sBuilder.Append(t.ToString("x2"));
                    }
                    var hash = sBuilder.ToString();

                    sql =
                        $" SELECT COUNT(id) FROM terminal_archive.users AS u WHERE u.name = '{name}' AND u.pass = '{hash}';";
                }
                if (contextConn == null)
                    conn.Open();
                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    users = dataReader.GetInt32(0);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return users > 0;
        }

        public static bool UserInRole(string name, string role, MySqlConnection contextConn = null)
        {
            var users = 0;
            if (name != null && role != null)
            {
                string sql =
                    $@" SELECT COUNT(u.id) FROM terminal_archive.users AS u 
 LEFT JOIN terminal_archive.roles AS rl ON u.id_role = rl.id
 LEFT JOIN terminal_archive.role_rights AS rr ON rr.id_role = rl.id
 LEFT JOIN terminal_archive.rights AS rg ON rr.id_right = rg.id
 WHERE u.name = '{name}' AND rg.name = '{role}';";
                var conn = contextConn ?? new MySqlConnection(ConnStr);
                if (contextConn == null)
                    conn.Open();
                var countCommand = new MySqlCommand(sql, conn);
                try
                {
                    var dataReader = countCommand.ExecuteReader();
                    while (dataReader.HasRows && dataReader.Read())
                    {
                        users = dataReader.GetInt32(0);
                    }
                    dataReader.Close();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (contextConn == null)
                        conn.Close();
                }
            }
            return users > 0;
        }

        public static bool UserIsAdmin(string name, MySqlConnection contextConn = null)
        {
            var group = UserTerminalGroup(name, contextConn);
            if (!UserInRole(name, "Write", contextConn) || !UserInRole(name, "Read", contextConn) || group != null)
                return false;

            return true;
        }

        public static int? UserTerminalGroup(string name, MySqlConnection contextConn = null)
        {
            int? result = -1;
            if (string.IsNullOrWhiteSpace(name))
                return result;

            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                var sql =
$@" SELECT r.id_group FROM terminal_archive.users AS u
 LEFT JOIN terminal_archive.roles AS r ON u.id_role = r.id
 WHERE u.name = '{name}';";
                if (contextConn == null)
                    conn.Open();
                var command = new MySqlCommand(sql, conn);

                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    result = dataReader.IsDBNull(0) ? null : (int?) dataReader.GetInt32(0);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static int TerminalsCount(string userName)
        {
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();
                if (!UserInRole(userName, "Read", conn))
                    throw new Exception("Unauthorize operation!");

                var group = UserTerminalGroup(userName, conn);
                var sql = " SELECT COUNT(t.id) FROM terminal_archive.terminals AS t ";
                if (group != null)
                    sql +=
$@" LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal
 WHERE tg.id_group = {group};";
                else
                    sql += ";";

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    result = dataReader.GetInt32(0);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                result = -1;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int OrdersCount(string userName, int idTerminal)
        {
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();
                if (!UserInRole(userName, "Read", conn))
                    throw new Exception("Unauthorize operation!");

                var group = UserTerminalGroup(userName, conn);
                string sql =
$@" SELECT COUNT(id) FROM terminal_archive.orders AS o ";
                if (group != null)
                    sql +=
$@" LEFT JOIN terminal_archive.terminals AS t ON t.id = o.id_terminal
 LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal ";
                sql +=
$@" WHERE o.id_terminal = {idTerminal} ";
                if (group != null)
                    sql += $@" AND tg.id_group = {group}";
                sql += ";";

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    result =  dataReader.GetInt32(0);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                result = -1;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool UpdateAllGroups(string user)
        {
            bool result = false;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (string.IsNullOrWhiteSpace(user) || !UserInRole(user, "Read", conn) )
                    throw new Exception("Unauthorize operation!");

                string sql =
$@" SELECT g.`id`, g.name
 FROM terminal_archive.groups AS g";
                if (group != null && group >= 0)
                    sql +=
$@" WHERE g.id = {group} ";
                sql +=
$@" ORDER BY g.id asc; ";
                MySqlCommand command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                Groups.Clear();
                while (dataReader.HasRows && dataReader.Read())
                {
                    Groups.Add(new Group()
                    {
                        Id = dataReader.GetInt32(0),
                        Name = dataReader.GetString(1),
                    });
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool UpdateTerminals(string userName, int currentPageTerminal, int pageSize, bool all = false)
        {
            bool result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();
                if (!UserInRole(userName, "Read", conn))
                {
                    Terminals.Clear();
                    throw new Exception("Unauthorize operation!");
                }

                var group = UserTerminalGroup(userName, conn);

                string sql =
$@" SELECT t.`id`, t.`name`, t.`address` , t.`id_hasp`
 FROM terminal_archive.terminals AS t ";
                if (group != null)
                    sql += $@" WHERE t.id IN (SELECT tg.id_terminal FROM terminal_archive.terminal_groups AS tg WHERE tg.id_group = {group}) ";

                sql += $@" ORDER BY t.id asc ";

                if (!all)
                    sql += $@" LIMIT {(currentPageTerminal - 1) * pageSize},{pageSize}";

                sql += $@" ;";


                var command = new MySqlCommand(sql, conn);
                Terminals.Clear();
                var dataReader = command.ExecuteReader();
                //int? lastTerminalId = null;
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idTerminal = dataReader.GetInt32(0);
                    if (!Terminals.ContainsKey(idTerminal))
                    {
                        //if (lastTerminalId != null)
                        //    Terminals[lastTerminalId.Value].Groups = groups;
                        //lastTerminalId = idTerminal;
                        //groups = new Dictionary<int, Group>();
                        Terminals[idTerminal] = new Terminal()
                        {
                            Id = idTerminal,
                            Name = dataReader.GetString(1),
                            Address = dataReader.GetString(2),
                            IdHasp = dataReader.GetString(3),
                            Orders = new Dictionary<int, Order>(),
                            Parameters = new List<Parameter>()
                        };
                    }
                    //if (!dataReader.IsDBNull(2))
                    //{
                    //    var grId = dataReader.GetInt32(2);
                    //    if (groups != null && !groups.ContainsKey(grId))
                    //        groups.Add(grId, new Group
                    //        {
                    //            Id = grId,
                    //            Name = dataReader.GetString(3),
                    //        });
                    //}
                }
                //if (lastTerminalId != null)
                //    Terminals[lastTerminalId.Value].Groups = groups;
                dataReader.Close();

                foreach(var terminalId in Terminals.Keys)
                { 
                    sql =
$@" SELECT g.`id`, g.`name`
 FROM terminal_archive.terminal_groups AS tg
 LEFT JOIN terminal_archive.groups AS g ON tg.id_group = g.id 
 WHERE tg.id_terminal = {terminalId}";
                    command = new MySqlCommand(sql, conn);
                    dataReader = command.ExecuteReader();
                    //Dictionary<int, Group> groups = null;
                    //int? lastTerminalId = null;
                    Dictionary<int, Group> groups = new Dictionary<int, Group>();
                    while (dataReader.HasRows && dataReader.Read())
                    {
                        if (!dataReader.IsDBNull(0))
                        {
                            var grId = dataReader.GetInt32(0);
                            if (groups != null && !groups.ContainsKey(grId))
                                groups.Add(grId, new Group
                                {
                                    Id = grId,
                                    Name = dataReader.GetString(1),
                                });
                        }
                    }
                    dataReader.Close();
                    Terminals[terminalId].Groups = groups;
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool UpdateTerminalOrders(string userName, int idTerminal, int currentPageOrder, int pageSize)
        {
            //if (!Terminals.Any())
            //    UpdateTerminals(userName, currentPageTerminal, pageSize);
            bool result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();
                if (!UserInRole(userName, "Read", conn))
                    throw new Exception("Unauthorize operation!");

                var group = UserTerminalGroup(userName, conn);
                string sql =
                    $@" SELECT o.`id`, `RNN`, s.id, s.name AS `состояние`, t.id, t.`name` AS `терминал` ,
 d.id, d.description AS `доп. параметр`, od.value AS `значение`,
 f.id, f.`name` AS `топливо` , p.id, p.`name` AS `оплата` , o.id_pump AS `колонка`,  
 `pre_price` ,  `price` ,  `pre_quantity` ,  `quantity` ,  `pre_summ` ,  `summ` FROM terminal_archive.orders AS o
 LEFT JOIN terminal_archive.order_fuels AS f ON o.id_fuel = f.id
 LEFT JOIN terminal_archive.order_payment_types AS p ON o.id_payment = p.id
 LEFT JOIN terminal_archive.terminals AS t ON o.id_terminal = t.id
 LEFT JOIN terminal_archive.order_states AS s ON o.id_state = s.id
 LEFT JOIN terminal_archive.order_details AS od ON o.id = od.id_order
 LEFT JOIN terminal_archive.details AS d ON od.id_detail = d.id";
                if (group != null)
                    sql +=
$@" LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal ";
                sql +=
$@" WHERE t.id = {idTerminal} ";
                if (group != null)
                    sql +=
$@" AND tg.id_group = {group} ";
                sql +=
$@" ORDER BY o.id desc LIMIT {(currentPageOrder - 1) * pageSize},{pageSize};";

                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                var orders = new Dictionary<int, Order>();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var orderId = dataReader.GetInt32(0);
                    if (!orders.ContainsKey(orderId))
                    {
                        var order = new Order()
                        {
                            Id = orderId,
                            Rnn = dataReader.GetString(1),
                            IdState = dataReader.GetInt32(2),
                            StateName = dataReader.GetString(3),
                            IdTerminal = dataReader.GetInt32(4),
                            TerminalName = dataReader.GetString(5),
                            AdditionalParameters = new List<AdditionalParameter>(),
                            IdFuel = dataReader.GetInt32(9),
                            FuelName = dataReader.GetString(10),
                            IdPayment = dataReader.GetInt32(11),
                            PaymentName = dataReader.GetString(12),
                            IdPump = dataReader.GetInt32(13),
                            PrePrice = dataReader.GetDecimal(13),
                            Price = dataReader.GetDecimal(13),
                            PreQuantity = dataReader.GetDecimal(13),
                            Quantity = dataReader.GetDecimal(13),
                            PreSumm = dataReader.GetDecimal(13),
                            Summ = dataReader.GetDecimal(13),
                        };
                        orders[orderId] = order;
                    }
                    if (!dataReader.IsDBNull(6))
                    {
                        var additionalParameter = new AdditionalParameter();
                        additionalParameter.Id = dataReader.GetInt32(6);
                        additionalParameter.IdOrder = orderId;
                        additionalParameter.Name = dataReader.GetString(7);
                        additionalParameter.Value = dataReader.GetString(8);
                        orders[orderId].AdditionalParameters.Add(additionalParameter);
                    }
                }
                Terminals[idTerminal].Orders = orders;
                dataReader.Close();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool UpdateTerminalParameters(string userName, int idTerminal)
        {
            bool result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();
                var group = UserTerminalGroup(userName, conn);
                if (!UserInRole(userName, "Write", conn) || !UserInRole(userName, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                string sql =
                $@" SELECT p.id AS `id параметра`, p.name AS `имя параметра`, p.path AS `путь параметра` ,tp.value AS `значение параметра`, 
 tp.last_edit_date, tp.save_date
 FROM terminal_archive.terminals AS t
 LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
 LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
 WHERE t.id = {idTerminal} /*tp.save_date < tp.last_edit_date*/
 ORDER BY p.id desc;";

                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                var parameters = new List<Parameter>();
                while (dataReader.HasRows && dataReader.Read())
                {
                    if (dataReader.IsDBNull(0))
                        continue;
                    var parameterId = dataReader.GetInt32(0);
                    parameters.Add(new Parameter()
                    {
                        Id = parameterId,
                        TId = idTerminal,
                        Name = dataReader.GetString(1),
                        Path = dataReader.GetString(2),
                        Value = dataReader.GetString(3),
                        LastEditTime = DateTime.ParseExact(dataReader.GetString(4), "dd.MM.yyyy HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture),
                        SaveTime = DateTime.ParseExact(dataReader.GetString(5), "dd.MM.yyyy HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture),
                    });
                }
                Terminals[idTerminal].Parameters = parameters;
                dataReader.Close();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="haspId">Указывается для определения терминала</param>
        /// <param name="rrn">Указывается для связи с заказом, может быть пустым</param>
        /// <param name="trace">Информация о месте возникновения ошибки</param>
        /// <param name="msg">Сообщение об ошибке</param>
        /// <param name="errorLevel">Уровень важности ошибки, в случае связи с заказом оставить пустым! (там будет текущее состояние заказа)</param>
        /// <param name="user">Данные для авторизации</param>
        /// <param name="pass">Пароль для авторизации</param>
        /// <returns></returns>
        public static bool AddTerminal(
            string haspId, string name, string address,
            string user/*, string pass*/
        )
        {
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (string.IsNullOrWhiteSpace(user) || !UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT t.id FROM terminal_archive.terminals AS t
 WHERE t.id_hasp = '{haspId}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int terminal = -1;
                while (reader.Read())
                    terminal = reader.GetInt32(0);
                reader.Close();

                if (terminal > 0)
                    throw new Exception("Terminal already exist!");

                string addSql = string.Format(
@" INSERT INTO `terminal_archive`.`terminals`
(`id_hasp`, `address`, `name`) 
VALUES ('{0}', '{1}', '{2}');", haspId, address, name);

                var addCommand = new MySqlCommand(addSql, conn);
                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = 0;
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static bool UpdateGroups(
            IEnumerable<TerminalGroup> to_add, IEnumerable<TerminalGroup> to_delete,
            string user/*, string pass*/
        )
        {
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (string.IsNullOrWhiteSpace(user) || !UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn))
                    throw new Exception("Unauthorize operation!");

                if (to_delete != null && to_delete.Any())
                {
                    string t_grp_del = string.Empty;
                    int cnt = 0;
                    foreach (var tg in to_delete)
                    {
                        if (cnt != 0)
                            t_grp_del += " OR ";

                        t_grp_del += $" (tg.`id_terminal`='{tg.IdTerminal}' AND tg.`id_group`='{tg.IdGroup}') ";
                        ++cnt;
                    }

                    string deleteSql =
$@" DELETE tg FROM `terminal_archive`.`terminal_groups` AS tg
 WHERE {t_grp_del} ;";
                    var deleteCommand = new MySqlCommand(deleteSql, conn);
                    int deleted = deleteCommand.ExecuteNonQuery();

                    if (deleted < to_delete.Count())
                        throw new Exception("Not all groups deleted!");
                }

                if (to_add != null && to_add.Any())
                {
                    string t_grp_add = string.Empty;
                    int cnt = 0;
                    foreach (var tg in to_add)
                    {
                        if (cnt != 0)
                            t_grp_add += " , ";

                        t_grp_add += $" ('{tg.IdTerminal}', '{tg.IdGroup}') ";
                        ++cnt;
                    }

                    string addSql =
$@" INSERT INTO `terminal_archive`.`terminal_groups` 
 (`id_terminal`, `id_group`) VALUES {t_grp_add} ;";
                    var addCommand = new MySqlCommand(addSql, conn);
                    int added = addCommand.ExecuteNonQuery();

                    if (added < to_add.Count())
                        throw new Exception("Not all groups added!");
                }

                result = to_add.Count() + to_delete.Count();
            }
            catch (Exception ex)
            {
                result = 0;
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="haspId">Указывается для определения терминала</param>
        /// <param name="rrn">Указывается для связи с заказом, может быть пустым</param>
        /// <param name="trace">Информация о месте возникновения ошибки</param>
        /// <param name="msg">Сообщение об ошибке</param>
        /// <param name="errorLevel">Уровень важности ошибки, в случае связи с заказом оставить пустым! (там будет текущее состояние заказа)</param>
        /// <param name="user">Данные для авторизации</param>
        /// <param name="pass">Пароль для авторизации</param>
        /// <returns></returns>
        public static bool AddHistory(
            string haspId, string rrn,
            string trace, string msg, int? errorLevel,
            string user, string pass
        )
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)
                || !IsAuthorizeUser(user, pass))
                return false;
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (!UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT t.id FROM terminal_archive.terminals AS t
 WHERE t.id_hasp = '{haspId}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int terminal = -1;
                while (reader.Read())
                    terminal = reader.GetInt32(0);
                reader.Close();

                if (terminal <= 0)
                    throw new Exception("Wrong terminal Hasp!");

                selectSql =
$@" SELECT o.id, o.id_state FROM terminal_archive.orders AS o 
 WHERE o.RNN = '{rrn??""}';";

                selectCommand = new MySqlCommand(selectSql, conn);
                reader = selectCommand.ExecuteReader();
                var order = -1;
                var state = -1;
                reader.Read();
                if (!string.IsNullOrWhiteSpace(rrn) && reader.HasRows && !reader.IsDBNull(0))
                    order = reader.GetInt32(0);

                if (errorLevel != null && errorLevel > 0)
                    state = 1000 + errorLevel.Value;
                else if (!string.IsNullOrWhiteSpace(rrn) && reader.HasRows && !reader.IsDBNull(1))
                    state = reader.GetInt32(1);
                reader.Close();

                //if (order <= 0)
                //    throw new Exception("Wrong order (rrn)!");
                //if (state <= 0)
                //    throw new Exception("Wrong state (rrn)!");

                string addSql = string.Format(
@" INSERT INTO
 `history` (`id_terminal`{0}{1}{2},`msg`)
 VALUES
 ('{3}'{4}{5}{6},'{7}');", 
order < 0 ? "" : ",`id_order`",
state < 0 ? "" : ",`id_state`",
string.IsNullOrWhiteSpace(trace) ? "" : ",`trace`",
terminal,
order < 0 ? "" : $",'{order}'",
state < 0 ? "" : $",'{state}'",
string.IsNullOrWhiteSpace(trace) ? "" : $",'{trace}'",
msg);

                var addCommand = new MySqlCommand(addSql, conn);
                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = 0;
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static IEnumerable<Parameter> GetParameters(string haspId, string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)
                || !IsAuthorizeUser(user, pass))
                return null;

            List<Parameter> parameters = new List<Parameter>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (!UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                string sql =
$@" SELECT t.`id`, 
 p.id AS `id параметра`, p.name AS `имя параметра` ,p.path AS `путь параметра`, 
 tp.value AS `значение параметра`, 
 tp.last_edit_date, tp.save_date
 FROM terminal_archive.terminals AS t
 LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
 LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
 WHERE tp.save_date < tp.last_edit_date AND t.id_hasp = '{haspId}' /*AND t.id IN (SELECT tg.id_terminal FROM terminal_archive.terminal_groups AS tg WHERE tg.id_group = {group})*/
 ORDER BY t.id asc; ";
                MySqlCommand command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    parameters.Add(new Parameter()
                    {
                        Id = dataReader.GetInt32(1),
                        TId = dataReader.GetInt32(0),
                        Name = dataReader.GetString(2),
                        Path = dataReader.GetString(3),
                        Value = dataReader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                parameters = null;
            }
            finally
            {
                conn.Close();
            }
            return parameters;
        }


        public static int UpdateSaveDate(int id, int parId, string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)
                || !IsAuthorizeUser(user, pass))
                return -1;

                var result = -1;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (!UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                var numberFormatInfo = new System.Globalization.CultureInfo("en-Us", false).NumberFormat;
                numberFormatInfo.NumberGroupSeparator = "";
                numberFormatInfo.NumberDecimalSeparator = ".";

                var now = DateTime.Now.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");

                string updateSql =
$@" UPDATE terminal_archive.terminal_parameters 
 SET save_date = '{now}' 
 WHERE id_terminal = '{id}' AND id_parameter = '{parId}';";

                var updateCommand = new MySqlCommand(updateSql, conn);
                result = updateCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool AddNewOrder(
            string rrn,//
            string haspId,
            int fuel,
            int pump,
            int payment,
            int state,
            decimal prePrice,
            decimal price,
            decimal preQuantity,
            decimal quantity,
            decimal preSumm,
            decimal summ,
            string user, string pass
            )
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)
                || !IsAuthorizeUser(user, pass))
                return false;

            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var group = UserTerminalGroup(user, conn);
                if (!UserInRole(user, "Write", conn) || !UserInRole(user, "Read", conn) || group != null)
                    throw new Exception("Unauthorize operation!");

                var numberFormatInfo = new System.Globalization.CultureInfo("en-Us", false).NumberFormat;
                numberFormatInfo.NumberGroupSeparator = "";
                numberFormatInfo.NumberDecimalSeparator = ".";

                string selectSql =
$@" SELECT count(o.id), t.id FROM terminal_archive.terminals AS t
 LEFT JOIN terminal_archive.orders AS o   ON o.id_terminal = t.id 
 WHERE t.hasp_id = '{haspId}' AND o.RNN = '{rrn}';";
  //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var orders = reader.GetInt32(0);
                var terminal = -1;
                if (orders > 0)
                {
                    terminal = reader.GetInt32(1);
                    reader.Close();
                }
                else
                {
                    reader.Close();
                    selectSql =
$@" SELECT t.id FROM terminal_archive.terminals AS t
 WHERE t.id_hasp = '{haspId}';";
                    selectCommand = new MySqlCommand(selectSql, conn);
                    reader = selectCommand.ExecuteReader();
                    reader.Read();
                    terminal = reader.GetInt32(0);
                    reader.Close();
                }
                var addSql = String.Empty;
                if (orders > 0)
                {
                    addSql =
$@" UPDATE `orders` AS o SET
 `id_state`={state},
 `pre_price`={prePrice.ToString(numberFormatInfo)},
 `price`={price.ToString(numberFormatInfo)},
 `pre_quantity`={preQuantity.ToString(numberFormatInfo)},
 `quantity`={quantity.ToString(numberFormatInfo)},
 `pre_summ`={preSumm.ToString(numberFormatInfo)},
 `summ`={summ.ToString(numberFormatInfo)}
 WHERE 
 o.id_terminal = {terminal} AND o.RNN = '{rrn}';";
                }
                else
                {
                    addSql =
$@" INSERT INTO
 `orders` (`id_terminal`,`RNN`,`id_fuel`,`id_pump`,`id_payment`,`id_state`,`pre_price`,`price`,`pre_quantity`,`quantity`,`pre_summ`,`summ`)
 VALUES
 ({terminal},'{rrn}',{fuel},{pump},{payment},{state},{prePrice.ToString(numberFormatInfo)},{price.ToString(numberFormatInfo)},{preQuantity.ToString(numberFormatInfo)},{quantity.ToString(numberFormatInfo)},{preSumm.ToString(numberFormatInfo)},{summ.ToString(numberFormatInfo)})";
                }
                var addCommand = new MySqlCommand(addSql, conn);

                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }
    }
}