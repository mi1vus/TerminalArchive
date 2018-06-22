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

        //public static Dictionary<int, Terminal> Terminals = new Dictionary<int, Terminal>();
        //public static List<Group> Groups = new ListUser<Group>();
        //public static Dictionary<int, User> Users = new Dictionary<int, User>();
        //public static Dictionary<int, Role> Roles = new Dictionary<int, Role>();

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
                string sql;
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
$@" SELECT MIN(rg.name not like 'None') FROM terminal_archive.users AS u
 LEFT JOIN terminal_archive.user_roles AS ur ON u.id = ur.id_user 
 LEFT JOIN terminal_archive.roles AS rl ON ur.id_role = rl.id
 LEFT JOIN terminal_archive.role_rights AS rr ON rr.id_role = rl.id
 LEFT JOIN terminal_archive.rights AS rg ON rr.id_right = rg.id
 WHERE u.name = '{name}' AND u.pass = '{hash}' ; ";
                }
                if (contextConn == null)
                    conn.Open();
                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
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

        public static bool UserInRole(string name, string role, int? group, MySqlConnection contextConn = null)
        {
            var users = 0;
            if (name != null && role != null)
            {
                string groupToQuery = (group == null ? " IS null " : $" = '{group}' ");
                string sql =
                    $@" SELECT COUNT(u.id) FROM terminal_archive.users AS u 
 LEFT JOIN terminal_archive.user_roles AS ur ON u.id = ur.id_user 
 LEFT JOIN terminal_archive.roles AS rl ON ur.id_role = rl.id
 LEFT JOIN terminal_archive.role_rights AS rr ON rr.id_role = rl.id
 LEFT JOIN terminal_archive.rights AS rg ON rr.id_right = rg.id
 WHERE u.name = '{name}' AND rg.name = '{role}' AND rl.id_group {groupToQuery} AND rg.name <> 'None' ; ";
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
            //var groups = GetUserGroups(name, "Read", contextConn);
            //groups.AddRange(GetUserGroups(name, "Write", contextConn));
            if (!UserInRole(name, "Write", null, contextConn) || !UserInRole(name, "Read", null, contextConn) /*|| (groups != null && groups.Any())*/)
                return false;

            return true;
        }

        public static List<Group> GetUserGroups(string name, string right, MySqlConnection contextConn = null)
        {
            var result = new List<Group>();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                var sql =
$@" SELECT g.id, g.name FROM terminal_archive.users AS u
 LEFT JOIN terminal_archive.user_roles AS ur ON u.id = ur.id_user 
 LEFT JOIN terminal_archive.roles AS r ON ur.id_role = r.id
 LEFT JOIN terminal_archive.groups AS g ON r.id_group = g.id
 LEFT JOIN terminal_archive.role_rights AS rr ON rr.id_role = r.id
 LEFT JOIN terminal_archive.rights AS rg ON rr.id_right = rg.id
WHERE u.name = '{name}' AND rg.name = '{right}'; ";
                if (contextConn == null)
                    conn.Open();
                var command = new MySqlCommand(sql, conn);

                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    if (!dataReader.IsDBNull(0))
                        result.Add(new Group {
                            Id = dataReader.GetInt32(0),
                            Name = dataReader.GetString(1),
                        });
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

        public static List<Group> GetAllGroups(string user)
        {
            var allGroups = new List<Group>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var groups = GetUserGroups(user, "Read", conn);

                string sql =
@" SELECT g.`id`, g.name
 FROM terminal_archive.groups AS g ";
                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql +=
$@" WHERE g.id in ( {groupStr} ) ";
                }
                sql +=
@" ORDER BY g.id asc; ";
                MySqlCommand command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    allGroups.Add(new Group()
                    {
                        Id = dataReader.GetInt32(0),
                        Name = dataReader.GetString(1),
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return allGroups;
        }

        public static Dictionary<int, User> GetAllUsers(string user)
        {
            var users = new Dictionary<int, User>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                const string sql = @" SELECT u.`id`, u.`name`, r.`id`, r.`name`, r.`id_group`
 FROM terminal_archive.users AS u
 LEFT JOIN terminal_archive.user_roles AS ur ON ur.id_user = u.id
 LEFT JOIN terminal_archive.roles AS r ON ur.id_role = r.id
 ORDER BY u.id asc; ";
                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idUser = dataReader.GetInt32(0);
                    if (!users.ContainsKey(idUser))
                    {
                        users[idUser] = new User
                        {
                            Id = idUser,
                            Name = dataReader.GetString(1),
                            Roles = new List<Role>()
                        };
                    }

                    if (dataReader.IsDBNull(2)) continue;

                    var rId = dataReader.GetInt32(2);
                    users[idUser].Roles .Add( new Role
                    {
                        Id = rId,
                        Name = dataReader.GetString(3),
                        IdGroup = dataReader.IsDBNull(4) ? null: (int?)dataReader.GetInt32(4),
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return users;
        }

        public static Dictionary<int, Role> GetAllRoles(string user)
        {
            var roles = new Dictionary<int, Role>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                const string sql = @" SELECT r.`id`, r.`name`, r.`id_group`, rg.`id`, rg.`name`
 FROM terminal_archive.roles AS r
 LEFT JOIN terminal_archive.role_rights AS rr ON rr.id_role = r.id
 LEFT JOIN terminal_archive.rights AS rg ON rr.id_right = rg.id
 ORDER BY r.id asc; ";
                MySqlCommand command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idRole = dataReader.GetInt32(0);
                    if (!roles.ContainsKey(idRole))
                    {
                        roles[idRole] = new Role
                        {
                            Id = idRole,
                            Name = dataReader.GetString(1),
                            IdGroup = dataReader.IsDBNull(2) ? null : (int?)dataReader.GetInt32(2),
                            Rights = new List<Right>()
                        };
                    }

                    if (dataReader.IsDBNull(3)) continue;

                    var rId = dataReader.GetInt32(3);
                    roles[idRole].Rights.Add(new Right
                    {
                        Id = rId,
                        Name = dataReader.GetString(4),
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return roles;
        }

        public static List<Right> GetAllRights(string user)
        {
            var rights = new List<Right>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                const string sql =
@" SELECT rg.`id`, rg.`name`
 FROM terminal_archive.rights AS rg
 ORDER BY rg.id asc; ";
                MySqlCommand command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idRight = dataReader.GetInt32(0);
                    rights.Add(new Right
                    {
                        Id = idRight,
                        Name = dataReader.GetString(1),
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return rights;
        }

        public static bool AddUser(
            string name, string pass,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT u.id FROM terminal_archive.users AS u
 WHERE u.name = '{name}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int userCnt = -1;
                while (reader.Read())
                    userCnt = reader.GetInt32(0);
                reader.Close();

                if (userCnt > 0)
                    throw new Exception("User already exist!");
                string password;
                using (var md5Hash = MD5.Create())
                {
                    var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    var sBuilder = new StringBuilder();
                    foreach (var t in data)
                    {
                        sBuilder.Append(t.ToString("x2"));
                    }
                    password = sBuilder.ToString();
                }

                string addSql = $@" INSERT INTO `terminal_archive`.`users`
(`name`, `pass`) 
VALUES ('{name}', '{password}'); ";

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

        public static bool EditUser(int id,
            string name, string oldPass, string pass,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                if (!IsAuthorizeUser(name, oldPass, conn))
                    throw new Exception("Incorrect user name or password!");

                string password;
                using (var md5Hash = MD5.Create())
                {
                    var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    var sBuilder = new StringBuilder();
                    foreach (var t in data)
                    {
                        sBuilder.Append(t.ToString("x2"));
                    }
                    password = sBuilder.ToString();
                }

                string selectSql =
$@" SELECT u.id FROM terminal_archive.users AS u
 WHERE u.id = '{id}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int userCnt = -1;
                while (reader.Read())
                    userCnt = reader.GetInt32(0);
                reader.Close();

                if (userCnt < 0)
                    throw new Exception($"No user with id={id}!");

                string updateSql = string.Format(
$@" UPDATE `terminal_archive`.`users` AS u
 SET `name` = '{name}',`pass` = '{password}'
 WHERE u.`id` = '{id}' ; ");

                var updateCommand = new MySqlCommand(updateSql, conn);
                result = updateCommand.ExecuteNonQuery();
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

        public static bool AddRole(
            string name, int? group,
            string user/*, string pass*/
)
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT r.id FROM terminal_archive.roles AS r
 WHERE r.name = '{name}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int roleCnt = -1;
                while (reader.Read())
                    roleCnt = reader.GetInt32(0);
                reader.Close();

                if (roleCnt > 0)
                    throw new Exception("Role already exist!");

                var groupStr = group == null ? " null " : $" '{group}' ";
                string addSql = $@" INSERT INTO `terminal_archive`.`roles`
(`name`, `id_group`) 
VALUES ('{name}', {groupStr}); ";

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

        public static bool EditRole(int id,
            string name, int? group,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT r.id FROM terminal_archive.roles AS r
 WHERE r.id = '{id}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int userCnt = -1;
                while (reader.Read())
                    userCnt = reader.GetInt32(0);
                reader.Close();

                if (userCnt < 0)
                    throw new Exception($"No user with id={id}!");

                var groupStr = group == null ? " null " : $" '{group}' ";
                string updateSql = string.Format(
$@" UPDATE `terminal_archive`.`roles` AS r
 SET `name` = '{name}', `id_group` = {groupStr}
 WHERE r.`id` = '{id}' ; ");

                var updateCommand = new MySqlCommand(updateSql, conn);
                result = updateCommand.ExecuteNonQuery();
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

        public static bool AddGroup(
            string name,
            string user/*, string pass*/
)
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT g.id FROM terminal_archive.groups AS g
 WHERE g.name = '{name}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int roleCnt = -1;
                while (reader.Read())
                    roleCnt = reader.GetInt32(0);
                reader.Close();

                if (roleCnt > 0)
                    throw new Exception("Group already exist!");

                string addSql = $@" INSERT INTO `terminal_archive`.`groups`
(`name`) 
VALUES ('{name}'); ";

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

        public static bool EditGroup(int id,
            string name,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT g.id FROM terminal_archive.groups AS g
 WHERE g.id = '{id}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int userCnt = -1;
                while (reader.Read())
                    userCnt = reader.GetInt32(0);
                reader.Close();

                if (userCnt < 0)
                    throw new Exception($"No group with id={id}!");

                string updateSql = string.Format(
$@" UPDATE `terminal_archive`.`groups` AS g
 SET `name` = '{name}'
 WHERE g.`id` = '{id}' ; ");

                var updateCommand = new MySqlCommand(updateSql, conn);
                result = updateCommand.ExecuteNonQuery();
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

        public static int TerminalsCount(string userName)
        {
            var result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var groups = GetUserGroups(userName, "Read", conn);
                var sql = " SELECT COUNT(t.id) FROM terminal_archive.terminals AS t ";
                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql +=
$@" WHERE t.id_group in ( {groupStr} ); ";
                }
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
                var groups = GetUserGroups(userName, "Read", conn);
                var sql =
@" SELECT COUNT(id) FROM terminal_archive.orders AS o ";
                if (groups != null && groups.Any())
                    sql +=
@" LEFT JOIN terminal_archive.terminals AS t ON t.id = o.id_terminal ";
                sql +=
$@" WHERE o.id_terminal = {idTerminal} ";
                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql += $@" AND t.id_group in ( {groupStr} ) ";
                }
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

        public static Dictionary<int, Terminal> GetTerminals(string userName, int currentPageTerminal, int pageSize, bool all = false)
        {
            var terminals = new Dictionary<int, Terminal>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var groups = GetUserGroups(userName, "Read", conn);

                var sql =
@" SELECT t.`id`, g.`id`, g.`name`, t.`name`, t.`address` , t.`id_hasp`
 FROM terminal_archive.terminals AS t 
 LEFT JOIN terminal_archive.groups AS g ON g.id = t.id_group ";
                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql +=
$@" WHERE t.id_group in ( {groupStr} ) ";
                }

                sql += @" ORDER BY t.id asc ";

                if (!all)
                    sql += $@" LIMIT {(currentPageTerminal - 1) * pageSize},{pageSize} ";

                sql += " ; ";


                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idTerminal = dataReader.GetInt32(0);
                    if (!terminals.ContainsKey(idTerminal))
                    {
                        terminals[idTerminal] = new Terminal()
                        {
                            Id = idTerminal,
                            Name = dataReader.GetString(3),
                            Address = dataReader.GetString(4),
                            IdHasp = dataReader.GetString(5),
                            Orders = new Dictionary<int, Order>(),
                            Parameters = new List<Parameter>()
                        };
                        if (!dataReader.IsDBNull(1))
                        {
                            var grId = dataReader.GetInt32(1);
                            terminals[idTerminal].IdGroup = grId;
                            terminals[idTerminal].Group = new Group
                            {
                                Id = grId,
                                Name = dataReader.GetString(2),
                            };
                        }
                    }
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return terminals;
        }

        public static Terminal GetTerminal(int id, string userName)
        {
            Terminal terminal = null;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                var groups = GetUserGroups(userName, "Read", conn);

                var sql =
$@" SELECT t.`id`, g.`id`, g.`name`, t.`name`, t.`address` , t.`id_hasp`
 FROM terminal_archive.terminals AS t 
 LEFT JOIN terminal_archive.groups AS g ON g.id = t.id_group 
 WHERE t.id = {id} ";

                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql +=
$@" AND t.id_group in ( {groupStr} ) ";
                }
                sql += @" ORDER BY t.id asc ";
                sql += " ; ";
                
                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read())
                {
                    var idTerminal = dataReader.GetInt32(0);
                    if (terminal == null)
                    {
                        terminal = new Terminal()
                        {
                            Id = idTerminal,
                            Name = dataReader.GetString(3),
                            Address = dataReader.GetString(4),
                            IdHasp = dataReader.GetString(5),
                            Orders = new Dictionary<int, Order>(),
                            Parameters = new List<Parameter>()
                        };
                        if (!dataReader.IsDBNull(1))
                        {
                            var grId = dataReader.GetInt32(1);
                            terminal.IdGroup = grId;
                            terminal.Group = new Group
                            {
                                Id = grId,
                                Name = dataReader.GetString(2),
                            };
                        }
                    }
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return terminal;
        }

        public static Dictionary<int, Order> GetTerminalOrders(string userName, int idTerminal, int currentPageOrder, int pageSize)
        {
            var orders = new Dictionary<int, Order>();
            var conn = new MySqlConnection(ConnStr);
            try 
            {
                conn.Open();

                var groups = GetUserGroups(userName, "Read", conn);
                var sql =
                    @" SELECT o.`id`, `RNN`, s.id, s.name AS `состояние`, t.id, t.`name` AS `терминал` ,
 d.id, d.description AS `доп. параметр`, od.value AS `значение`,
 f.id, f.`name` AS `топливо` , p.id, p.`name` AS `оплата` , o.id_pump AS `колонка`,  
 `pre_price` ,  `price` ,  `pre_quantity` ,  `quantity` ,  `pre_summ` ,  `summ` FROM terminal_archive.orders AS o
 LEFT JOIN terminal_archive.order_fuels AS f ON o.id_fuel = f.id
 LEFT JOIN terminal_archive.order_payment_types AS p ON o.id_payment = p.id
 LEFT JOIN terminal_archive.terminals AS t ON o.id_terminal = t.id
 LEFT JOIN terminal_archive.order_states AS s ON o.id_state = s.id
 LEFT JOIN terminal_archive.order_details AS od ON o.id = od.id_order
 LEFT JOIN terminal_archive.details AS d ON od.id_detail = d.id";
                sql +=
$@" WHERE t.id = {idTerminal} ";
                if (groups != null && groups.Any())
                {
                    var groupStr = groups.Select(t => t.Id.ToString())
                        .Aggregate((current, next) => current + ", " + next);

                    sql +=
$@" AND t.id_group in ( {groupStr} ) ";
                }
                sql +=
$@" ORDER BY o.id desc LIMIT {(currentPageOrder - 1) * pageSize},{pageSize};";

                var command = new MySqlCommand(sql, conn);
                var dataReader = command.ExecuteReader();
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
                        var additionalParameter = new AdditionalParameter
                        {
                            Id = dataReader.GetInt32(6),
                            IdOrder = orderId,
                            Name = dataReader.GetString(7),
                            Value = dataReader.GetString(8)
                        };
                        orders[orderId].AdditionalParameters.Add(additionalParameter);
                    }
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return orders;
        }

        public static List<Parameter> GetTerminalParameters(string userName, int idTerminal)
        {
            var parameters = new List<Parameter>();
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

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
                while (dataReader.HasRows && dataReader.Read())
                {
                    if (dataReader.IsDBNull(0))
                        continue;
                    var parameterId = dataReader.GetInt32(0);
                    //var t1 = dataReader.GetString(4);
                    //var tt1 = dataReader.GetDateTime(4);
                    //var t2 = dataReader.GetString(5);
                    //var tt2 = DateTime.ParseExact(t2, "dd.mm.yyyy HH:mm:ss",
                    //    System.Globalization.CultureInfo.InvariantCulture);

                    parameters.Add(new Parameter()
                    {
                        Id = parameterId,
                        TId = idTerminal,
                        Name = dataReader.GetString(1),
                        Path = dataReader.GetString(2),
                        Value = dataReader.GetString(3),
                        LastEditTime = dataReader.GetDateTime(4),
                        SaveTime = dataReader.GetDateTime(5),
                    });
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="haspId">Указывается для определения терминала</param>
        /// <param name="groupId">номер групповой принадлежности терминала</param>
        /// <param name="address"></param>
        /// <param name="user">Данные для авторизации</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool AddTerminal(
            string haspId, int groupId, string name, string address,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
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

                var groupTxt = groupId > 0 ? $"'{groupId}'" : "null";

                string addSql = $@" INSERT INTO `terminal_archive`.`terminals`
(`id_hasp`, `id_group`, `address`, `name`) 
VALUES ('{haspId}', {groupTxt}, '{address}','{name}') ; ";

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="haspId">Указывается для определения терминала</param>
        /// <param name="address"></param>
        /// <param name="user">Данные для авторизации</param>
        /// <param name="groupId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool EditTerminal(int id,
            string haspId, int groupId, string name, string address,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string selectSql =
$@" SELECT t.id FROM terminal_archive.terminals AS t
 WHERE t.id = '{id}';";
                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                int terminal = -1;
                while (reader.Read())
                    terminal = reader.GetInt32(0);
                reader.Close();

                if (terminal < 0)
                    throw new Exception($"No terminal with id={id}!");

                string groupTxt = groupId > 0 ? $"'{groupId}'" : "null"; 

                string updateSql = string.Format(
$@" UPDATE `terminal_archive`.`terminals` AS t
 SET `id_hasp` = '{haspId}',`id_group` = {groupTxt}, `address` = '{address}', `name` = '{name}'
 WHERE t.`id` = '{id}' ;");

                var updateCommand = new MySqlCommand(updateSql, conn);
                result = updateCommand.ExecuteNonQuery();
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

        public static bool UpdateUserRoles(
            IEnumerable<UserRole> toAdd, IEnumerable<UserRole> toDelete,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                var userRolesToDel = toDelete as UserRole[] ?? toDelete.ToArray();
                if (userRolesToDel.Any())
                {
                    string uRlsDel = string.Empty;
                    int cnt = 0;
                    foreach (var ur in userRolesToDel)
                    {
                        if (cnt != 0)
                            uRlsDel += " OR ";

                        uRlsDel += $" (ur.`id_user`='{ur.IdUser}' AND ur.`id_role`='{ur.IdRole}') ";
                        ++cnt;
                    }

                    string deleteSql =
$@" DELETE ur FROM `terminal_archive`.`user_roles` AS ur
 WHERE {uRlsDel} ;";
                    var deleteCommand = new MySqlCommand(deleteSql, conn);
                    var deleted = deleteCommand.ExecuteNonQuery();

                    if (deleted < userRolesToDel.Length)
                        throw new Exception("Not all roles deleted!");
                }

                var userRolesToAdd = toAdd as UserRole[] ?? toAdd.ToArray();
                if (userRolesToAdd.Any())
                {
                    var uRolAdd = string.Empty;
                    var cnt = 0;
                    foreach (var ur in userRolesToAdd)
                    {
                        if (cnt != 0)
                            uRolAdd += " , ";

                        uRolAdd += $" ('{ur.IdUser}', '{ur.IdRole}') ";
                        ++cnt;
                    }

                    string addSql =
$@" INSERT INTO `terminal_archive`.`user_roles` 
 (`id_user`, `id_role`) VALUES {uRolAdd} ;";
                    var addCommand = new MySqlCommand(addSql, conn);
                    int added = addCommand.ExecuteNonQuery();

                    if (added < userRolesToAdd.Length)
                        throw new Exception("Not all roles added!");
                }

                result = userRolesToAdd.Count() + userRolesToDel.Count();
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

        public static bool UpdateRoleRights(
            IEnumerable<RoleRight> toAdd, IEnumerable<RoleRight> toDelete,
            string user/*, string pass*/
        )
        {
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                var roleRightsToDel = toDelete as RoleRight[] ?? toDelete.ToArray();
                if (roleRightsToDel.Any())
                {
                    string rrDel = string.Empty;
                    int cnt = 0;
                    foreach (var rr in roleRightsToDel)
                    {
                        if (cnt != 0)
                            rrDel += " OR ";

                        rrDel += $" (rr.`id_role`='{rr.IdRole}' AND rr.`id_right`='{rr.IdRight}') ";
                        ++cnt;
                    }

                    string deleteSql =
$@" DELETE rr FROM `terminal_archive`.`role_rights` AS rr
 WHERE {rrDel} ;";
                    var deleteCommand = new MySqlCommand(deleteSql, conn);
                    var deleted = deleteCommand.ExecuteNonQuery();

                    if (deleted < roleRightsToDel.Length)
                        throw new Exception("Not all rights deleted!");
                }

                var roleRightsToAdd = toAdd as RoleRight[] ?? toAdd.ToArray();
                if (roleRightsToAdd.Any())
                {
                    var rrAdd = string.Empty;
                    var cnt = 0;
                    foreach (var rr in roleRightsToAdd)
                    {
                        if (cnt != 0)
                            rrAdd += " , ";

                        rrAdd += $" ('{rr.IdRole}', '{rr.IdRight}') ";
                        ++cnt;
                    }

                    string addSql =
$@" INSERT INTO `terminal_archive`.`role_rights` 
 (`id_role`, `id_right`) VALUES {rrAdd} ;";
                    var addCommand = new MySqlCommand(addSql, conn);
                    int added = addCommand.ExecuteNonQuery();

                    if (added < roleRightsToAdd.Length)
                        throw new Exception("Not all rights added!");
                }

                result = roleRightsToAdd.Count() + roleRightsToDel.Count();
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
        /// <param name="date"></param>
        /// <param name="user">Данные для авторизации</param>
        /// <param name="pass">Пароль для авторизации</param>
        /// <returns></returns>
        public static bool AddHistory(
            string haspId, string rrn,
            string trace, string msg, int? errorLevel, string date,
            string user, string pass
        )
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)
                || !IsAuthorizeUser(user, pass))
                return false;
            int result;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                if (!UserIsAdmin(user, conn))
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

                

                var addSql = $@" INSERT INTO
 `history` (
`id_terminal`, 
`date`, 
{(order < 0 ? "" : ",`id_order`")}{(state < 0 ? "" : ",`id_state`")}{(string
                    .IsNullOrWhiteSpace(trace)
                    ? ""
                    : ",`trace`")},
`msg`)
 VALUES
 (
'{terminal}',
'{date}'
{(order < 0 ? "" : $",'{order}'")}{(state < 0 ? "" : $",'{state}'")}{(string.IsNullOrWhiteSpace(
                    trace)
                    ? ""
                    : $",'{trace}'")},
'{msg}');";

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

                if (!UserIsAdmin(user, conn))
                    throw new Exception("Unauthorize operation!");

                string sql =
$@" SELECT t.`id`, 
 p.id AS `id параметра`, p.name AS `имя параметра` ,p.path AS `путь параметра`, 
 tp.value AS `значение параметра`, 
 tp.last_edit_date, tp.save_date
 FROM terminal_archive.terminals AS t
 LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
 LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
 WHERE tp.save_date < tp.last_edit_date AND t.id_hasp = '{haspId}' /*AND t.id IN (SELECT tg.id_terminal FROM terminal_archive.terminal_groups AS tg WHERE tg.id_group = )*/
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

                if (!UserIsAdmin(user, conn))
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

                if (!UserIsAdmin(user, conn))
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
                string addSql;
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