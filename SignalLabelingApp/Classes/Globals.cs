using Avalonia.Controls;
using SignalLabelingApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SignalLabelingApp.Classes
{
    public class Globals
    {
        public static EditorBase CurrentEditor { get; set; }
        public static ObservableCollection<DatasetSample> AllDatasetSamples { get; set; } = new ObservableCollection<DatasetSample>();
        private static int _currentSampleId = 0;
        public static int CurrentSampleId
        {
            get
            {
                return _currentSampleId;
            }

            set 
            {
                if (_currentSampleId != value)
                {
                    _currentSampleId = value;
                }
            }
        }

        public static Control CurrentEditorMetadata { get; set; }
        public static Control MainEditorControl { get; set; }

        public static Dictionary<string, EditorBase> FileExtentionToEditor { get; set; } = new Dictionary<string, EditorBase>()
        {
            { ".mseed", new MiniseedEditor() },
            { ".miniseed", new MiniseedEditor() },
            { ".seed", new MiniseedEditor() },
        };

        public static int GenerateUniqueId()
        {
            return CurrentSampleId += 1;
        }
    }
}
