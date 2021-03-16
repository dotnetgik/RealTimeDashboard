using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ChartJs.Blazor;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorDashboard.LineChartDashBoard
{
	public class RealTimeTableDashboard : ComponentBase
	{

		private const int InitalCount = 7;
		protected LineConfig _config;
		

		protected HubConnection hubConnection; //for connecting to SignalR
		protected readonly string functionAppBaseUri = "http://localhost:7071/api/"; //URL for function app. Leave this as is for now.

		public List<DashboardMessage> messages = new List<DashboardMessage>();

		readonly IDataset<int> _dataSetForPune = new LineDataset<int>(new List<int>(InitalCount))
		{
			Label = "Pune",
			BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
			BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
			Fill = FillingMode.Disabled
		};


		readonly IDataset<int> _dataSetForMumbai = new LineDataset<int>(new List<int>(InitalCount))
		{
			Label = "Mumbai",
			BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
			BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
			Fill = FillingMode.Disabled

		};

		protected override async Task OnInitializedAsync()
		{
			SetConfig();

			hubConnection = new HubConnectionBuilder()
						.WithUrl(functionAppBaseUri)
						.Build();
			Connect();

			await hubConnection.StartAsync(); //start connection!

			foreach (var time in SampleUtils.TimeofTheDay)
			{
				_config.Data.Labels.Add(time);
			}
			_config.Data.Datasets.Add(_dataSetForPune);
			_config.Data.Datasets.Add(_dataSetForMumbai);
		}
		
		private void AddData()
		{
			if (_config.Data.Datasets.Count == 0)
				return;

			var month = SampleUtils.TimeofTheDay[_config.Data.Labels.Count % SampleUtils.TimeofTheDay.Count];
			_config.Data.Labels.Add(month);
		}


		private void SetConfig()
		{
			_config = new LineConfig
			{
				Options = new LineOptions
				{
					Responsive = true,
					Title = new OptionsTitle
					{
						Display = true,
						Text = "Temperature per hour"
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
			hubConnection.On<List<DashboardMessage>>("dashboardMessage", (clientMessage) =>
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

		public bool IsConnected => hubConnection.State == HubConnectionState.Connected;
	}
}
