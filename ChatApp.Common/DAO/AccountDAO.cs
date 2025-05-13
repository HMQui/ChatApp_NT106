using ChatApp.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Common.DAO
{
    public class AccountDAO
    {
        private static AccountDAO instance;
        public static AccountDAO Instance
        {
            get { if (instance == null) instance = new AccountDAO(); return instance; }
            private set { instance = value; }
        }
        private AccountDAO() { }

        public bool CheckAccount(string email, string password)
        {
            string query = "SELECT password FROM Users WHERE email = @param0";
            var parameters = new object[] { email };

            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters);

            if (dataTable.Rows.Count > 0)
            {
                string hashedPassword = dataTable.Rows[0]["password"].ToString();

                return HandlePassword.VerifyPassword(password, hashedPassword);
            }

            return false;
        }

        public bool CheckExistEmail(string email)
        {
            string query = "SELECT email FROM Users WHERE email = @param0";
            var parameters = new object[] { email };
            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters);
            return dataTable.Rows.Count > 0;
        }

        public int InsertAccountNotVerify(string email, string password, string randomCode, string name)
        {
            string query = "INSERT INTO Users (email, password, verify_code, full_name) VALUES (@param0, @param1, @param2, @param3)";
            var parameters = new object[] { email, HandlePassword.HashPassword(password), randomCode, name };
            return DataProvider.Instance.ExcuteNonQuery(query, parameters);
        }

        public int DeleteByConditions(Dictionary<string, object> conditions)
        {

            StringBuilder queryBuilder = new StringBuilder("DELETE FROM Users WHERE ");
            List<object> parameters = new List<object>();

            int index = 0;
            foreach (var condition in conditions)
            {
                if (index > 0)
                    queryBuilder.Append(" AND ");

                queryBuilder.Append($"{condition.Key} = @param{index}");
                parameters.Add(condition.Value);
                index++;
            }

            string query = queryBuilder.ToString();
            return DataProvider.Instance.ExcuteNonQuery(query, parameters.ToArray());
        }

        public int UpdateFields(Dictionary<string, object> fieldsToUpdate, Dictionary<string, object> conditions)
        {
            StringBuilder queryBuilder = new StringBuilder("UPDATE Users SET ");

            var parameters = new List<object>();
            int paramIndex = 0;

            // SET clause
            foreach (var field in fieldsToUpdate)
            {
                if (paramIndex > 0)
                    queryBuilder.Append(", ");
                queryBuilder.Append($"{field.Key} = @param{paramIndex}");
                parameters.Add(field.Value);
                paramIndex++;
            }

            queryBuilder.Append(" WHERE ");
            int whereIndex = 0;
            foreach (var condition in conditions)
            {
                if (whereIndex > 0)
                    queryBuilder.Append(" AND ");
                queryBuilder.Append($"{condition.Key} = @param{paramIndex}");
                parameters.Add(condition.Value);
                paramIndex++;
                whereIndex++;
            }

            string query = queryBuilder.ToString();
            return DataProvider.Instance.ExcuteNonQuery(query, parameters.ToArray());
        }

        public List<UserDTO> SearchUsersByEmail(string keyword)
        {
            string query = "SELECT id, email, full_name, avatar_url, status, created_at, is_verified " +
                           "FROM Users WHERE email LIKE @param0";
            string pattern = $"%{keyword}%";
            var parameters = new object[] { pattern };

            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters);
            var result = new List<UserDTO>();

            foreach (DataRow row in dataTable.Rows)
            {
                var user = new UserDTO
                {
                    Id = Convert.ToInt32(row["id"]),
                    Email = row["email"].ToString(),
                    FullName = row["full_name"].ToString(),
                    AvatarUrl = row["avatar_url"] != DBNull.Value ? row["avatar_url"].ToString() : null,
                    Status = row["status"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["created_at"]),
                    IsVerified = Convert.ToBoolean(row["is_verified"])
                };
                result.Add(user);
            }

            return result;
        }

        public UserDTO GetUserInfoByEmail(string email)
        {
            string query = "SELECT id, email, full_name, avatar_url, status, created_at, is_verified FROM Users WHERE email = @param0";
            var parameters = new object[] { email };
            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters);

            var result = new UserDTO();
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                result.Id = Convert.ToInt32(row["id"]);
                result.Email = row["email"].ToString();
                result.FullName = row["full_name"].ToString();
                result.AvatarUrl = row["avatar_url"] != DBNull.Value ? row["avatar_url"].ToString() : null;
                result.Status = row["status"].ToString();
                result.CreatedAt = Convert.ToDateTime(row["created_at"]);
                result.IsVerified = Convert.ToBoolean(row["is_verified"]);
            }
            return result;

        }
    }
}
