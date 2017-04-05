﻿using System;
using System.Windows.Forms;
using Terminals.Data;
using Terminals.Forms.EditFavorite;

namespace Terminals.Plugins.Putty
{
    public partial class SshOptionsControl : PuttyOptionsControl, IProtocolOptionsControl
    {
        public SshOptionsControl()
        {
            this.InitializeComponent();

            this.cmbSshVersion.DataSource = Enum.GetValues(typeof(SshVersion));
        }

        protected override void LoadFrom(ProtocolOptions protocolOptions)
        {
            var sshOptions = protocolOptions as SshOptions;
            base.LoadFrom(sshOptions);

            if (null != sshOptions)
            {
                this.checkBoxX11Forwarding.Checked = sshOptions.X11Forwarding;
                this.checkBoxCompression.Checked = sshOptions.EnableCompression;
                this.checkBoxEnablePagentForwarding.Checked = sshOptions.EnablePagentForwarding;
                this.checkBoxEnablePagentAuthentication.Checked = sshOptions.EnablePagentAuthentication;
                this.cmbSshVersion.Text = sshOptions.SshVersion.ToString();
            }
        }

        protected override void SaveTo(ProtocolOptions protocolOptions)
        {
            var sshOptions = protocolOptions as SshOptions;
            base.SaveTo(sshOptions);

            if (null != sshOptions)
            {
                sshOptions.X11Forwarding = this.checkBoxX11Forwarding.Checked;
                sshOptions.EnableCompression = this.checkBoxCompression.Checked;
                sshOptions.EnablePagentForwarding = this.checkBoxEnablePagentForwarding.Checked;
                sshOptions.EnablePagentAuthentication = this.checkBoxEnablePagentAuthentication.Checked;
                sshOptions.SshVersion = (SshVersion) Enum.Parse(typeof(SshVersion), cmbSshVersion.SelectedValue.ToString());
            }
        }
    }
}
