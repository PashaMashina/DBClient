using System.Data;
using System.Data.SqlClient;

namespace DBClient
{
    public partial class GenerateForm : Form
    {
        public class Word
        {
            public string word;
            public string gender;
            
        }

        public class Range 
        {
            public double min;
            public double max;

            public bool CheckPercent(double value)
            { 
                return min <= value && value <= max;
            }
        }

        public string SurnameGender(string word)
        {
            string gender;
            string[] extensions = { "���", "��", "�", "��", "��", "�", "�", "�", "�", "�", "�", "�" };
            bool endsWord = extensions.Any(suffix => word.EndsWith(suffix));
            if (endsWord)
            {
                gender = "�";
            }
            else
            {
                gender = "�";
            }
            return gender;
        }

        public GenerateForm()
        {
            InitializeComponent();

            this.names = File.ReadAllLines("names.csv").Select(x =>
            {
                var word_gender = x.Split(';');

                return new Word
                {
                    word = word_gender[0],
                    gender = word_gender[1],
                };
            }).ToArray();

            this.surnames = File.ReadAllLines("surnames.csv").Select(x =>
            {
                var word_string = x;
                var word_gender = SurnameGender(x);

                return new Word
                {
                    word = word_string,
                    gender = word_gender,
                };
            }).ToArray();

            this.titleRaces = File.ReadAllLines("races.csv").Select(x =>
            {
                var word_gender = x;

                return new Word
                {
                    word = x,
                };
            }).ToArray();
        }

        private SqlConnection GetSqlConnection()
        {
            SqlConnection connection = new();
            SqlConnectionStringBuilder sb = new();
            sb.DataSource = @"(LocalDB)\MSSQLLocalDB";
            sb.AttachDBFilename = @"C:\Users\evtuh\OneDrive\������� ����\DriverRaces.mdf";
            sb.IntegratedSecurity = true;
            sb.ConnectTimeout = 30;

            connection.ConnectionString = sb.ToString();

            return connection;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand(txtQuery.Text, connection);
                try { 
                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        dataGridMain.DataSource = table;
                    }
                }catch (SqlException ex)
                {
                    MessageBox.Show("������ � �������", "SQL �������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        Random random = new();
        private Word[] names;
        private Word[] surnames;
        private Word[] locationRaces;
        private Word[] titleRaces;

        private string GenerateDriverName(string gender)
        {
            var genderNames = this.names.Where(x => x.gender == gender).ToList();

            var name = genderNames.ElementAt(random.Next(0, genderNames.Count()));

            return $"{name.word}";
        }

        private string GenerateDriverSurname(string gender)
        {
            var genderSurnames = this.surnames.Where(x => x.gender == gender);


            var surname = genderSurnames.ElementAt(random.Next(0, genderSurnames.Count()));

            return $"{surname.word}";
        }

        private void btnGenerationDrivers_Click(object sender, EventArgs e)
        {
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                pbDriverGeneration.Value = 0; // ���������� ��������
                pbDriverGeneration.Maximum = (int)upCount.Value; // ������ ������������

                var classifications = new Dictionary<string, TrackBar>
                {
                    ["����������"] = tbPlatinum,
                    ["�������"] = tbGold,
                    ["�����������"] = tbSilver,
                    ["���������"] = tbBronze,
                };

                int totalClassification = classifications.Values.Sum(tb => tb.Value);

                Dictionary<string, Range> classificationProbablity = new();
                double previousProbabilty = 0;

                foreach (var item in classifications)
                {
                    double k = (double)classifications[item.Key].Value / totalClassification;
                    if (k > 0)
                    {
                        classificationProbablity[item.Key] = new Range
                        {
                            min = previousProbabilty,
                            max = previousProbabilty + k
                        };

                        previousProbabilty += k;
                    }
                }

                Random rnd = new();

                for (var i = 0; i < upCount.Value; ++i)
                {
                    string classification = null;
                    var k = rnd.NextDouble(); // ������� ��������� ����� �� 0 �� 1

                    // �������� �� ����� ������������
                    foreach (var item in classificationProbablity)
                    {
                        // ���� �������� �������� � ���������� 
                        if (item.Value.CheckPercent(k))
                        {
                            classification = item.Key; // �� ��������� ��������
                            break; // � ������� �� �����
                        }
                    }

                    if (classification == null)
                        continue;


                    var isMale = ((double)tbMaleFemale.Value / 100) < rnd.NextDouble();
                    var gender = isMale ? "�" : "�";
                    var name = GenerateDriverName(gender);
                    var surname = GenerateDriverSurname(gender);


                    SqlCommand command = new($@"
INSERT INTO Drivers(name, surname, classification, sex) 
VALUES(N'{name}', N'{surname}', N'{classification}', N'{gender}')"
, connection);
                    command.ExecuteNonQuery();

                    pbDriverGeneration.Value++;
                }
            }
        }

        private void btnGenerationRaces_Click_1(object sender, EventArgs e)
        {
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                pbRaces.Value = 0; // ���������� ��������
                pbRaces.Maximum = (int)upCountRaces.Value; // ������ ������������

                var typeRaces = new Dictionary<string, TrackBar>
                {
                    ["��������� �����"] = tbCircle,
                    ["�����"] = tbRally,
                    ["�����"] = tbTrofy,
                    ["����� �� ������������"] = tbEndurance,
                    ["����-�������"] = tbDrug,
                };

                int totalTypeRaces = typeRaces.Values.Sum(tb => tb.Value);

                Dictionary<string, Range> typeRacesProbablity = new();
                double previousProbabilty = 0;

                foreach (var item in typeRaces)
                {
                    double k = (double)typeRaces[item.Key].Value / totalTypeRaces;
                    if (k > 0)
                    {
                        typeRacesProbablity[item.Key] = new Range
                        {
                            min = previousProbabilty,
                            max = previousProbabilty + k
                        };

                        previousProbabilty += k;
                    }
                }

                Random rnd = new();

                for (var i = 0; i < upCountRaces.Value; ++i)
                {
                    string typeRace = null;
                    var k = rnd.NextDouble(); // ������� ��������� ����� �� 0 �� 1

                    // �������� �� ����� ������������
                    foreach (var item in typeRacesProbablity)
                    {
                        // ���� �������� �������� � ���������� 
                        if (item.Value.CheckPercent(k))
                        {
                            typeRace = item.Key; // �� ��������� ��������
                            break; // � ������� �� �����
                        }
                    }

                    if (typeRace == null)
                        continue;

                    Random rnd2 = new();

                    var title = this.titleRaces.ElementAt(rnd2.Next(0, this.titleRaces.Count())).word;

                    Random rnd3 = new();

                    DateTime start = new DateTime(1970, 1, 1);
                    int range = (DateTime.Today - start).Days;
                    var dataRace = start.AddDays(rnd3.Next(range)).ToString("yyyy-MM-dd");

                    SqlCommand command = new($@"
DECLARE @id_location int;
set @id_location = (SELECT TOP 1 ID FROM Location
ORDER BY NEWID());
DECLARE @dates DATE;
SET @dates = CAST('{dataRace}' AS DATETIME2);
INSERT INTO Races(title, location_id, date, type) 
VALUES(N'{title}', @id_location, @dates, N'{typeRace}')"
, connection);
                    command.ExecuteNonQuery();

                    pbRaces.Value++;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                pbDrivRaceGeneration.Value = 0;
                pbDrivRaceGeneration.Maximum = (int)upCountDrivRac.Value;

                var val = upCountDrivRac.Value;
                ;
                SqlCommand command = new($@"
EXEC add_RacesToDrivers"
, connection);
                command.ExecuteNonQuery();
            }

            for (var i = 0; i < upCountDrivRac.Value; ++i)
            {
                pbDrivRaceGeneration.Value++;
            }
        }

        private void upCountDrivRac_ValueChanged(object sender, EventArgs e)
        {
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                var val = upCountDrivRac.Value;
                ;
                SqlCommand command = new($@"
ALTER PROCEDURE add_RacesToDrivers
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT row_number() OVER(ORDER BY id_race) AS ids, id_race INTO #race_ids FROM Races;

	DECLARE @race_id_amount BIGINT;
	SELECT @race_id_amount = COUNT(#race_ids.id_race) FROM #race_ids;

	

	SELECT row_number() OVER(ORDER BY Id) AS ids, Id INTO #driver_ids FROM Drivers;

	DECLARE @driver_id_amount BIGINT;
	SELECT @driver_id_amount = COUNT(#driver_ids.Id) FROM #driver_ids;

	DECLARE @amount INT;
	SET @amount = {val};

	WHILE @amount > 0
		BEGIN
			DECLARE @race_id_new BIGINT;
			SET @race_id_new = RAND() * @race_id_amount + 1;

			DECLARE @driver_id_new BIGINT;
			SET @driver_id_new = RAND() * @driver_id_amount + 1;

			DECLARE @race_id BIGINT;
			DECLARE @driver_id BIGINT;

			SELECT @race_id = #race_ids.id_race FROM #race_ids WHERE #race_ids.ids = @race_id_new;
			SELECT @driver_id = #driver_ids.Id FROM #driver_ids WHERE #driver_ids.ids = @driver_id_new;
			
			SELECT * INTO #rowd FROM RacesToDrivers WHERE race_id = @race_id AND driver_id = @driver_id;

			DECLARE @count TINYINT;
			SELECT @count = COUNT(#rowd.race_id) FROM #rowd;

			IF @count = 0
				INSERT INTO RacesToDrivers(race_id, driver_id, time_finish) VALUES(@race_id, @driver_id, DATEADD(s, ABS(CHECKSUM(NewId()) % 43201), CAST('08:00:00' AS Time)));

			SET @amount -= 1;
			DROP TABLE #rowd;
		END
		
			
		DROP TABLE #race_ids;
		DROP TABLE #driver_ids;
		
END"
, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
