﻿namespace DMU_Git.Models.DTO
{
    public class LogDTO
    {
        public LogParent LogParentDTOs { get; set; }
        public List<LogChild> ChildrenDTOs { get; set; }
    }
}
