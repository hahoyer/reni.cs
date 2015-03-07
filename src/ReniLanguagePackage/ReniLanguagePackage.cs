using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using hw.Debug;
using JetBrains.Annotations;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace HoyerWare.ReniLanguagePackage
{
    [ProvideService(typeof(ReniService), ServiceName = "Reni Language Service")]
    [ProvideLanguageService(typeof(ReniService),
        "Reni Language",
        106, // resource ID of localized language name
        CodeSense = true, // Supports IntelliSense
        RequestStockColors = false, // Supplies custom colors
        EnableCommenting = true, // Supports commenting out code
        EnableAsyncCompletion = true // Supports background parsing
        )]
    [ProvideLanguageExtension(typeof(ReniService), ".reni")]
    [Guid("F4FB62CF-3E45-4920-9721-89099113477E")]
    [UsedImplicitly]
    public sealed class ReniLanguagePackage : Package, IOleComponent
    {
        protected override void Initialize()
        {
            base.Initialize();

            var serviceContainer = this as IServiceContainer;
            var langService = new ReniService();
            langService.SetSite(this);
            serviceContainer.AddService(typeof(ReniService), langService, true);
        }


        int IOleComponent.FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        int IOleComponent.FDoIdle(uint grfidlef)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        int IOleComponent.FPreTranslateMessage(MSG[] pMsg)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        int IOleComponent.FQueryTerminate(int fPromptUser)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        int IOleComponent.FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        IntPtr IOleComponent.HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        void IOleComponent.OnActivationChange
            (
            IOleComponent pic,
            int fSameComponent,
            OLECRINFO[] pcrinfo,
            int fHostIsActivating,
            OLECHOSTINFO[] pchostinfo,
            uint dwReserved)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        void IOleComponent.OnAppActivate(int fActive, uint dwOtherThreadID)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        void IOleComponent.OnEnterState(uint uStateID, int fEnter)
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        void IOleComponent.OnLoseActivation()
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        void IOleComponent.Terminate()
        {
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }
    }
}