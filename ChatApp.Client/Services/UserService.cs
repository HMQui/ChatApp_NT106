using ChatApp.Common.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ChatApp.Client.Services
{
    public class UserService
    {
        public void SetOnlineStatusInDB(string email)
        {
            var updateFields = new Dictionary<string, object>
            {
                { "status", "online" }
            };
            var condition = new Dictionary<string, object>
            {
                { "email", email }
            };
            AccountDAO.Instance.UpdateFields(updateFields, condition);
        }

        public void SetOfflineStatusInDB(string email)
        {
            var updateFields = new Dictionary<string, object>
            {
                { "status", "offline" }
            };
            var condition = new Dictionary<string, object>
            {
                { "email", email }
            };
            AccountDAO.Instance.UpdateFields(updateFields, condition);
        }
    }
}
    
