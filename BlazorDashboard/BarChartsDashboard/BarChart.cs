using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorDashboard.BarChartsDashboard
{
    public class BarChart : ComponentBase
    {
        private const int InitialCount = 7;
        protected BarConfig _config;
        protected Random _rng = new Random();
        protected Chart _chart;
        private HubConnection HubConnection; //for connecting to SignalR

        private readonly string
            FunctionAppBaseUri = "http://localhost:7071/api/"; //URL for function app. Leave this as is for now.


        IDataset<int> dataset1 = new BarDataset<int>(new List<int>(InitialCount))
        {
            Label = "Pune",
            BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
            BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132))
        };


        readonly IDataset<int> dataset2 = new BarDataset<int>(new List<int>(InitialCount))
        {
            Label = "Mumbai",
            BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
            BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200))
        };

        protected override async Task OnInitializedAsync()
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl(FunctionAppBaseUri)
                .Build();

            Connect();

            _config = GetConfig();

            await HubConnection.StartAsync(); //start connection!

            foreach (var time in SampleUtils.TimeofTheDay)
            {
                _config.Data.Labels.Add(time);
            }

            _config.Data.Datasets.Add(dataset1);
            _config.Data.Datasets.Add(dataset2);
        }

        private static BarConfig GetConfig()
        {
            return new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = Position.Top
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Corona patient Reported per 15 min"
                    }
                }
            };
        }

        private void AddData()
        {
            if (_config.Data.Datasets.Count == 0)
                return;

            var month = SampleUtils.TimeofTheDay[_config.Data.Labels.Count % SampleUtils.TimeofTheDay.Count];
            _config.Data.Labels.Add(month);
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
                            dataset1.Add(Convert.ToInt16(message.Details));
                            break;
                        case "2":
                            dataset2.Add(Convert.ToInt16(message.Details));
                            break;
                    }
                }

                AddData();

                StateHasChanged(); //This tells Blazor that the UI needs to be updated
            });
        }
    }
}