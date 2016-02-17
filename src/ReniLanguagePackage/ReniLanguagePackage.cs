using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;
using Reni.Formatting;

namespace HoyerWare.ReniLanguagePackage
{
    [ProvideService(typeof(ReniService), ServiceName = "Reni Language Service")]
    [ProvideLanguageService(typeof(ReniService), Name, 106)]
    [ProvideLanguageExtension(typeof(ReniService), ".reni")]
    [Guid("F4FB62CF-3E45-4920-9721-89099113477E")]
    [ProvideLanguageEditorOptionPage(typeof(Properties), Name, "Advanced", "", "100")]
    [UsedImplicitly]
    public sealed class ReniLanguagePackage : Package
    {
        const string Name = "Reni";

        protected override void Initialize()
        {
            base.Initialize();

            var serviceContainer = this as IServiceContainer;
            var langService = new ReniService();
            langService.SetSite(this);
            serviceContainer.AddService(typeof(ReniService), langService, true);
            Application.Idle += OnIdle;
        }

        void OnIdle(object sender, EventArgs e)
            => (GetService(typeof(ReniService)) as ReniService)?.OnIdle(true);

        internal HierachicalFormatter CreateFormattingProvider()
        {
            var pd = (Properties) GetDialogPage(typeof(Properties));
            return new HierachicalFormatter
            {
                MaxLineLength = pd.MaxLineLength,
                EmptyLineLimit = pd.EmptyLineLimit,
            };
        }
    }
}