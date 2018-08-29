using System;

namespace api
{
    public class TodoItem
    {
		public string id { get; set; }
		public string ItemName { get; set; }
		public string ItemOwner { get; set; }
		public DateTime? ItemCreateDate { get; set; }
	}
}
