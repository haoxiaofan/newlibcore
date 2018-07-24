﻿using System;
using System.Collections.Generic;
using NewLibCore.Data.Mapper.MapperExtension;
using NewLibCore.Data.Mapper.PropertyExtension;

namespace UnitTestProject1
{
	internal class Account : PropertyMonitor
	{
		public Account()
		{
		}

		[DateTimeDefaultValue()]
		public DateTime AddTime { get; internal set; }

		[PropertyDefaultValue(typeof(String), "xiaofan")]
		public String Name { get; set; }

		public System.Int32 Id { get; internal set; }
		public System.Boolean IsDeleted { get; internal set; }
		public System.Boolean IsDisable { get; internal set; }

		public Boolean IsOnline { get; internal set; }

		public IList<Int32> RoleIds { get; internal set; }

		internal void Offline()
		{
			IsOnline = true;
			base.OnPropertyChanged(new PropertyArgs(nameof(IsOnline), IsOnline));
		}

		internal void ModifyName()
		{
			Name = "123456";
			OnPropertyChanged(new PropertyArgs(nameof(Name), Name));
		}

		internal void ModifyTime()
		{
			AddTime = DateTime.Now.AddDays(10);
			OnPropertyChanged(new PropertyArgs(nameof(AddTime), AddTime));
		}
	}

	internal class Account2 : PropertyMonitor
	{
		public Account2()
		{
		}

		public String Name { get; set; }

		public System.Int32 Id { get; internal set; }
		public System.Boolean IsDeleted { get; internal set; }
		public System.Boolean IsDisable { get; internal set; }

		public Boolean IsOnline { get; internal set; }

		internal void Offline()
		{
			IsOnline = true;
			//base.OnPropertyChanged(new PropertyArgs(nameof(IsOnline), IsOnline));
		}
	}
}