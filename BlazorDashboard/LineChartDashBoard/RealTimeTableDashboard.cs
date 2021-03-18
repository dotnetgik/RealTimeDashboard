using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace BlazorDashboard.LineChartDashBoard
{
    public class RealTimeTableDashboard : ComponentBase
    {
        private const int InitialCount = 0;
        protected LineConfig Config;


        protected HubConnection HubConnection; //for connecting to SignalR

        protected readonly string
            FunctionAppBaseUri = "http://localhost:7071/api/"; //URL for function app. Leave this as is for now.

        public List<DashboardMessage> Messages = new List<DashboardMessage>();

        readonly IDataset<int> _dataSetForPune = new LineDataset<int>(new List<int>(InitialCount))
        {
            Label = "Pune",
            BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
            BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
            Fill = FillingMode.Disabled
        };


        readonly IDataset<int> _dataSetForMumbai = new LineDataset<int>(new List<int>(InitialCount))
        {
            Label = "Mumbai",
            BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
            BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
            Fill = FillingMode.Disabled
        };

        protected override async Task OnInitializedAsync()
        {
            SetConfig();

            HubConnection = new HubConnectionBuilder()
                .WithUrl(FunctionAppBaseUri)
                .Build();
            Connect();

            await HubConnection.StartAsync(); //start connection!

            foreach (var time in SampleUtils.TimeofTheDay)
            {
                Config.Data.Labels.Add(time);
            }

            Config.Data.Datasets.Add(_dataSetForPune);
            Config.Data.Datasets.Add(_dataSetForMumbai);
        }

        private void AddData()
        {
            if (Config.Data.Datasets.Count == 0)
                return;

            var month = SampleUtils.TimeofTheDay[Config.Data.Labels.Count % SampleUtils.TimeofTheDay.Count];
            Config.Data.Labels.Add(month);
        }

        private void SetConfig()
        {
            Config = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Corona patients reported 15 min"
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = true
                    },
                    Hover = new Hover
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = true
                    },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
                        {
                            new CategoryAxis
                            {
                                ScaleLabel = new ScaleLabel
                                {
                                    LabelString = "Time"
                                }
                            }
                        },
                        YAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                ScaleLabel = new ScaleLabel
                                {
                                    LabelString = "Temp (in Celsius ) "
                                }
                            }
                        }
                    }
                }
            };
        }

        private void Connect()
        {
            HubConnection.On<List<DashboardMessage>>("dashboardMessage", (clientMessage) =>
            {
                foreach (var message in clientMessage)
                {
                    switch (message.Id)
                    {
                        case "1":
                            _dataSetForPune.Add(Convert.ToInt16(message.Details));
                            break;
                        case "2":
                            _dataSetForMumbai.Add(Convert.ToInt16(message.Details));
                            break;
                    }
                }

                AddData();

                StateHasChanged(); //This tells Blazor that the UI needs to be updated
            });
        }

        public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
    }
}