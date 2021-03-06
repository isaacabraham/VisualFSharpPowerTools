﻿using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using FSharpVSPowerTools.ProjectSystem;
using FSharpVSPowerTools.HighlightUsage;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace FSharpVSPowerTools
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("F#")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class HighlightUsageFilterProvider : IVsTextViewCreationListener
    {
        private readonly System.IServiceProvider _serviceProvider;
        private readonly IViewTagAggregatorFactoryService _tagAggregator;
        private readonly IVsEditorAdaptersFactoryService _editorFactory;
       
        [ImportingConstructor]
        public HighlightUsageFilterProvider(
            [Import(typeof(SVsServiceProvider))] System.IServiceProvider serviceProvider,
            IViewTagAggregatorFactoryService tagAggregator,
            IVsEditorAdaptersFactoryService editorFactory)
        {
            _serviceProvider = serviceProvider;
            _tagAggregator = tagAggregator;
            _editorFactory = editorFactory;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = _editorFactory.GetWpfTextView(textViewAdapter);
            if (textView == null) return;

            var generalOptions = Setting.getGeneralOptions(_serviceProvider);
            if (generalOptions == null || !generalOptions.HighlightUsageEnabled) return;
            
            AddCommandFilter(textViewAdapter,
                new HighlightUsageFilter(textView, _tagAggregator.CreateTagAggregator<TextMarkerTag>(textView)));            
        }

        private static void AddCommandFilter(IVsTextView viewAdapter, HighlightUsageFilter commandFilter)
        {
            if (!commandFilter.IsAdded)
            {
                // Get the view adapter from the editor factory
                IOleCommandTarget next;
                int hr = viewAdapter.AddCommandFilter(commandFilter, out next);

                if (hr == VSConstants.S_OK)
                {
                    commandFilter.IsAdded = true;
                    // You'll need the next target for Exec and QueryStatus
                    if (next != null) commandFilter.NextTarget = next;
                }
            }
        }
    }
}