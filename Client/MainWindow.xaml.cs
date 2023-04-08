using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client;

public partial class MainWindow : Window
{
	private bool xTurn;
	private int[,] board;

	private TcpClient client;
	private NetworkStream stream;
	private Button[,] buttons;
	private BinaryReader reader;
	private BinaryWriter writer;

	public MainWindow()
	{
		InitializeComponent();

		Initialize();
		Task.Run(ReceiveUpdates);
	}

	private void Initialize()
	{
		board = new int[3, 3];

		buttons = new Button[3, 3]
		{
			{ button00, button01, button02 },
			{ button10, button11, button12 },
			{ button20, button21, button22 }
		};

		client = new TcpClient("localhost", 1234);
		stream = client.GetStream();
		reader = new BinaryReader(stream);
		writer = new BinaryWriter(stream);

		xTurn = reader.ReadBoolean();

		if (xTurn)
			Title = "Player X";
		else
			Title = "Player O";
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		int row = Grid.GetRow(button);
		int column = Grid.GetColumn(button);

		if (xTurn)
		{
			button.Content = "X";
			board[row, column] = 1;
		}
		else
		{
			button.Content = "O";
			board[row, column] = 2;
		}

		button.IsEnabled = false;

		CheckForWinner();
		writer.Write(GetGameState());

		grid.IsEnabled = false;
	}

	private void ReceiveUpdates()
	{
		while (true)
		{
			Dispatcher.Invoke(() => { grid.IsEnabled = true; });

			string receivedData = reader.ReadString();
			UpdateGameState(receivedData);
		}
	}

	private string GetGameState()
	{
		string gameState = "";

		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				gameState += board[i, j];

		return gameState;
	}

	private void UpdateGameState(string gameState)
	{
		int index = 0;

		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int value = int.Parse(gameState[index].ToString());
				board[i, j] = value;

				if (value == 1)
				{
					Dispatcher.Invoke(() =>
					{
						buttons[i, j].Content = "X";
						buttons[i, j].IsEnabled = false;
					});
				}
				else if (value == 2)
				{
					Dispatcher.Invoke(() =>
					{
						buttons[i, j].Content = "O";
						buttons[i, j].IsEnabled = false;
					});
				}
				else
				{
					Dispatcher.Invoke(() =>
					{
						buttons[i, j].Content = "";
						buttons[i, j].IsEnabled = true;
					});
				}

				index++;
			}
		}
	}

	private void CheckForWinner()
	{
		for (int i = 0; i < 3; i++)
		{
			if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
			{
				ShowWinner(board[i, 0]);
				return;
			}
		}

		for (int i = 0; i < 3; i++)
		{
			if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
			{
				ShowWinner(board[0, i]);
				return;
			}
		}

		if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
		{
			ShowWinner(board[0, 0]);
			return;
		}

		if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
		{
			ShowWinner(board[0, 2]);
			return;
		}

		bool isTie = true;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (board[i, j] == 0)
				{
					isTie = false;
					break;
				}
			}

			if (!isTie)
				break;
		}

		if (isTie)
			ShowTie();
	}

	private void ShowWinner(int player)
	{
		MessageBox.Show($"Player {(player == 1 ? "X" : "O")} wins!");
		ResetGame();
	}

	private void ShowTie()
	{
		MessageBox.Show("It's a tie!");
		ResetGame();
	}

	private void ResetGame()
	{
		board = new int[3, 3];

		foreach (var element in grid.Children)
		{
			if (element is Button button)
			{
				button.Content = "";
				button.IsEnabled = true;
			}
		}
	}
}