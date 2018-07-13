using System;

namespace PackageVerification.SQLRunner.Models
{
    public class DatabaseObject
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public int SchemaId { get; set; }
        public int ParentObjectId { get; set; }
        public string Type { get; set; }
        public string TypeDesc { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string SchemaName { get; set; }
        public string FullSafeName {get { return "[" + SchemaName + "].[" + Name + "]"; }}
    }
}