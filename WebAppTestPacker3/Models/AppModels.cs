using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebAppTestPacker3.Models
{
    public class PerWebUserCache
    {
        [Key]
        public int EntryId { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }
}