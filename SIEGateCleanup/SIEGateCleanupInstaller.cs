﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace SIEGateCleanup
{
    [RunInstaller(true)]
    public partial class SIEGateCleanupInstaller : System.Configuration.Install.Installer
    {
        public SIEGateCleanupInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
        }
    }
}
