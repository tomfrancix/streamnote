﻿using System;
using System.Collections.Generic;

namespace Streamnote.Relational.Models.Descriptors
{
    public class UserNameDescriptor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserImageContentType { get; set; }
        public byte[] UserImage { get; set; }
    }
}
