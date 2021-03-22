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


		private HubConnection SignalRHubConnection; //for connecting to SignalR

		protected readonly string SignalServerUri = "http://localhost:7071/api/"; //URL for function app. Leave this as is for now.

		public List<DashboardMessage> Messages = new List<DashboardMessage>();

		/// <summary>
		/// Data set initialization for city 1 used for the chart component 
		/// </summary>
		readonly IDataset<int> _dataSetForPune = new LineDataset<int>(new List<int>(InitialCount))
		{
			Label = "Pune",
			BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
			BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 99, 132)),
			Fill = FillingMode.Disabled
		};


		/// <summary>
		/// Contains Metadata for chart for the city 2
		/// </summary>

		readonly IDataset<int> _dataSetForMumbai = new LineDataset<int>(new List<int>(InitialCount))
		{
			Label = "Mumbai",
			BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
			BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 125, 200)),
			Fill = FillingMode.Disabled
		};

		/// <summary>
		/// The method called when the component is loaded
		/// </summary>
		/// <returns></returns>
		protected override async Task OnInitializedAsync()
		{
			// Set the configuration for the Charts to be used this will be line chart in this case 
			SetConfig();

			// Build the Hub connection
			SignalRHubConnection = new HubConnectionBuilder()
				.WithUrl(SignalServerUri)
				.Build();

			Connect();

			await SignalRHubConnection.StartAsync(); //start connection!

			foreach (var time in SampleUtils.TimeofTheDay)
			{
				Config.Data.Labels.Add(time);
			}

			Config.Data.Datasets.Add(_dataSetForPune);
			Config.Data.Datasets.Add(_dataSetForMumbai);
		}

		private void AddData()
		{
			// check if the configuration has datasets
			if (Config.Data.Datasets.Count == 0)
				return;
			// Get the labls based on the dataset counts
			var timeOftheDay = SampleUtils.TimeofTheDay[Config.Data.Labels.Count % SampleUtils.TimeofTheDay.Count];
			Config.Data.Labels.Add(timeOftheDay);
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
						Text = "Temprature"
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
			//Register a handler which will be invoked when the hub method dashboard
			SignalRHubConnection.On<List<DashboardMessage>>("dashboardMessage", (clientMessage) =>
			{
				// when the message arrives update the client message an data set 
				foreach (var message in clientMessage)
				{
					// Update the dataset based on the city Id for demo we are using two datasets which will be used in the chart.
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

				// Adds the labels data coming from signalr Service to the Dataset 
				AddData();

				//This tells Blazor that the UI needs to be updated

				StateHasChanged();
			});
		}

		public bool IsConnected => SignalRHubConnection.State == HubConnectionState.Connected;
	}
}