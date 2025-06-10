using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
    public class GroupService
    {
        private readonly ChatGroupHub _chatGroupHub;

        public GroupService()
        {
            _chatGroupHub = new ChatGroupHub();
        }

        public async Task Register(string email)
        {
            await _chatGroupHub.Register(email);
        }

        public async Task JoinGroup(int groupId)
        {
            await _chatGroupHub.JoinGroup(groupId);
        }

        public async Task LeaveGroup(int groupId)
        {
            await _chatGroupHub.LeaveGroup(groupId);
        }

        public async Task SendGroupMessage(int groupId, byte[] data, string senderEmail, string messageType, DateTime sendAt, string originalFileName = "")
        {
            await _chatGroupHub.SendGroupMessage(groupId, data, senderEmail, messageType, sendAt, originalFileName);
        }

        public GroupDTO CreateGroup(string groupName, int createdBy)
        {
            return GroupDAO.Instance.CreateGroup(groupName, createdBy);
        }

        public bool AddMember(int groupId, int userId)
        {
            return GroupDAO.Instance.AddMember(groupId, userId);
        }

        public List<GroupDTO> GetUserGroups(int userId)
        {
            return GroupDAO.Instance.GetUserGroups(userId);
        }

        public List<GroupMemberDTO> GetGroupMembers(int groupId)
        {
            return GroupDAO.Instance.GetGroupMembers(groupId);
        }

        public bool RemoveMember(int groupId, int userId)
        {
            return GroupDAO.Instance.RemoveMember(groupId, userId);
        }

        public bool UpdateGroupInfo(int groupId, string groupName)
        {
            return GroupDAO.Instance.UpdateGroupInfo(groupId, groupName);
        }

        public GroupDTO GetGroupById(int groupId)
        {
            return GroupDAO.Instance.GetGroupById(groupId);
        }

        public async Task<bool> UpdateGroupName(int groupId, string newName)
        {
            return GroupDAO.Instance.UpdateGroupInfo(groupId, newName);
        }
    }
} 