Imports System.Data.SQLite
Imports System.Text

Module StreamSQL
    Public Function LoadStreams(ByVal limit As Integer) As StringBuilder
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\streams.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            limit = 10 * (limit - 1)

            sqlite_cmd.CommandText = "SELECT * FROM streams LIMIT " & limit & ",10;"

            Dim dr = sqlite_cmd.ExecuteReader()
            Dim sbuilder As New StringBuilder
            While dr.Read
                sbuilder.Append("• [" & dr("streamTitle").ToString & "](" & dr("streamURL").ToString & ") by " & dr("streamUser").ToString).AppendLine()
            End While

            Return sbuilder
            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Public Function StreamQuery(ByVal streamID As Integer, ByVal clm As String) As String
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\streams.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM streams WHERE ID = @id;"
            sqlite_cmd.Parameters.Add("@id", SqlDbType.Int).Value = streamID

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       [" & streamID & "] | " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Function StreamCount() As Integer
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\streams.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM streams;"

        Dim rc As Integer = Convert.ToInt32(sqlite_cmd.ExecuteScalar())

        Colorize("[SQL]       " & sqlite_cmd.CommandText)

        Return rc
    End Function

    Public Sub AddStream(ByVal streamURL As String, ByVal streamTitle As String, ByVal streamUser As String)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\streams.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "INSERT INTO streams (streamURL, streamTitle, streamUser) VALUES (@url, @title, @user);"
            sqlite_cmd.Parameters.Add("@url", SqlDbType.VarChar, 500).Value = streamURL
            sqlite_cmd.Parameters.Add("@title", SqlDbType.VarChar, 50).Value = streamTitle
            sqlite_cmd.Parameters.Add("@user", SqlDbType.VarChar, 50).Value = streamUser
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       [" & streamURL & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub

    Public Sub ClearStreams()
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\streams.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand
            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "DELETE FROM streams;"
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub
End Module
