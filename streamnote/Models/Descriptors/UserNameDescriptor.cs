﻿using System;
using System.Collections.Generic;

namespace streamnote.Models.Descriptors
{
    public class UserNameDescriptor
    {
        public string UserName { get; set; }
        public string UserImageContentType { get; set; }
        public byte[] UserImage { get; set; }
    }
}
