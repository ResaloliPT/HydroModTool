﻿using System;
using System.Collections.Generic;

namespace HydroModTools.Configuration.Models
{
    internal sealed class GeneralConfig
    {
        public List<ProjectConfig> Projects { get; set; } = new List<ProjectConfig>();

        public Guid? DefaultProject { get; set; }
    }

    internal sealed class ProjectConfig
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get; set; } = "";

        public string Path { get; set; } = "";

        public string OutputPath { get; set; } = "";

        public List<ProjectItemConfig> Items { get; set; } = new List<ProjectItemConfig>();
    }

    internal sealed class ProjectItemConfig
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get; set; } = "";

        public string Path { get; set; } = "";
    }
}