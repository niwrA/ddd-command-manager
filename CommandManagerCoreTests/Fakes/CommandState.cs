﻿using System;
using niwrA.CommandManager;
using niwrA.CommandManager.Contracts;

namespace CommandManagerCoreTests.Fakes
{
    public class CommandState : ICommandState
    {
        public string Command { get; set; }
        public string CommandVersion { get; set; }
        public string EntityGuid { get; set; }
        public string Entity { get; set; }
        public string EntityRootGuid { get; set; }
        public string EntityRoot { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public Guid Guid { get; set; }
        public string ParametersJson { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string UserId { get; set; }
        public string TransactionId { get; set; }
        public string ConfigurationId { get; set; }
    }
}
