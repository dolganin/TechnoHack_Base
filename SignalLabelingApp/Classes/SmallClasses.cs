﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace SignalLabelingApp.Classes
{

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////Классы вспомогательные для редактора////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    public class EditorMetadata
    {
        public string Name { get; set; }
        public string AmountOfStations { get; set; }
        public string Description { get; set; }
    }

    public class NamedRectangle : Rectangle
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public List<NamedRectangle> OrangeRectangles { get; set; } = new();

         public NamedRectangle(int id)
        {
            Id = id;
            Name = $"{id}";
        }

        public NamedRectangle(int id, double width, double height, IBrush fill) : base()
        {
            Id = id;
            Name = $"{id}";
            Width = width;
            Height = height;
            Fill = fill;
        }

        public void SetId(int id)
        {
            Id = id;
            Name = $"{id}";
        }
    }


    


    public class MiniseedFile
    {
        public string filePath = null;
        public int stationsAmount = 0;
        public ObservableCollection<StationData> stationDataStructures = new ObservableCollection<StationData>();

    }

    public class StationData
    {
        public string StationName;
        public TraceData Channel1 {  get; set; }
        public TraceData Channel2 { get; set; }
        public TraceData Channel3 { get; set; }
    }

    public class TraceData
    {
        public int SampleRate { get; set; }
        public string StationName { get; set; }
        public int NumberOfSamples { get; set; }
        public DateTime starttime { get; set; }
        public DateTime endtime { get; set; }
        public List<float> data = new List<float>();
    }

    public class DatasetSample
    {
        public Label Label { get; set; } // Хранит объект Label
        public TraceFragment TraceFragment { get; set; } // Фрагмент трёхканальной трассы
    }

    public class TraceFragment
    {
        public List<float> Channel1 { get; set; }
        public List<float> Channel2 { get; set; }
        public List<float> Channel3 { get; set; }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////Классы разметки////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class DetectionObject
    {
        public double X { get; set; }
        public double W { get; set; }
        public int Class { get; set; } 
    }

    [JsonDerivedType(typeof(SignalClassificationLabel), "classification")]
    [JsonDerivedType(typeof(SignalDetectionLabel), "detection")]
    [JsonDerivedType(typeof(SignalSegmentationLabel), "segmentation")]
    public abstract class Label
    {
        public int EventID;// = Globals.GenerateUniqueId();
    }

    public class SignalClassificationLabel : Label
    {
        public double ObjectStartPos;
        public double ObjectEndPos;
        public double ObjectClass { get; set; }
    }

    public class SignalDetectionLabel : Label
    {
        public double SignalStartPos;
        public double SignalEndPos;
     //   public ObservableCollection<DetectionObject> Objects { get; set; } = new();
        public Dictionary<int, DetectionObject> Objects {get; set; } = new();
       
    }

    public class SignalSegmentationLabel : Label
    {
        public double ObjectStartPos;
        public double ObjectEndPos;
    }


}
