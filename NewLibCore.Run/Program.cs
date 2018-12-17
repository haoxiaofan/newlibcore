﻿using NewLibCore.Data.SQL.BuildExtension;
using NewLibCore.Data.SQL.MapperExtension;
using NewLibCore.Data.SQL.PropertyExtension;
using System;

namespace NewLibCore.Run
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            var where = new BuilderWhere<VisitorRecord>();
        }

    }
    public partial class VisitorRecord : DomainModelBase
    {
        public Int32 UserId { get; private set; }

        [PropertyRequired, PropertyInputRange(10), PropertyDefaultValue(typeof(String), "11111")]
        public String UserName { get; private set; }

        public VisitorRecord(Int32 userId, String userName)
        {
            UserId = userId;
            UserName = userName;
        }

        public VisitorRecord() { }
    }
    public abstract class DomainModelBase : PropertyMonitor
    {
        protected DomainModelBase()
        {
            IsDeleted = false;
        }

        public Int32 Id { get; protected set; }

        [PropertyDefaultValue(typeof(Boolean), false)]
        public Boolean IsDeleted { get; protected set; }

        [DateTimeDefaultValue]
        public DateTime AddTime { get; protected set; }

        [DateTimeDefaultValue]
        public DateTime LastModifyTime { get; protected set; }

        public void Remove()
        {
            IsDeleted = true;
            OnPropertyChanged(new PropertyArgs(nameof(IsDeleted), IsDeleted));
        }
    }
}
