Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Discord
Imports Discord.WebSocket
Module StreamBot
    Sub Main(args As String())
        handler = New ConsoleEventDelegate(AddressOf ConsoleEventCallback)
        SetConsoleCtrlHandler(handler, True)
        ' Start main as an async sub
        MainAsync.GetAwaiter.GetResult()
    End Sub

    Public clientconfig As DiscordSocketConfig = New DiscordSocketConfig With {
        .TotalShards = 1
    }
    Public _client As DiscordShardedClient = New DiscordShardedClient(clientconfig)

    Sub New()
        ' Set console encoding for names with symbols like ♂️ and ♀️
        Console.OutputEncoding = Text.Encoding.UTF8
        ' Set our log, ready, timer, and message received functions
        AddHandler _client.Log, AddressOf LogAsync
        AddHandler _client.ShardReady, AddressOf ReadAsync
        AddHandler _client.MessageReceived, AddressOf MessageReceivedAsync
        AddHandler _client.ShardConnected, AddressOf ShardConnectedAsync
        AddHandler _client.ShardReady, AddressOf ShardReadyAsync
    End Sub

    <STAThread()>
    Public Async Function MainAsync() As Task
        ' Set thread
        Threading.Thread.CurrentThread.SetApartmentState(Threading.ApartmentState.STA)

        Await _client.LoginAsync(TokenType.Bot, "")

        ' Wait for the client to start
        Await _client.StartAsync
        Await Task.Delay(-1)
    End Function

    Private Async Function LogAsync(ByVal log As LogMessage) As Task(Of Task)
        ' Once loginasync and startasync finish we get the log message of "Ready" once we get that, we load everything else
        If log.ToString.Contains("Ready") Then
            Colorize("[GATEWAY]   " & log.ToString)

            Await _client.SetGameAsync(" god.")
        ElseIf log.ToString.Contains("gateway") Or log.ToString.Contains("unhandled") Then
        Else
            Colorize("[GATEWAY]   " & log.ToString) ' update console
        End If
        Return Task.CompletedTask
    End Function

    ' Async reader
    Private Async Function ReadAsync() As Task(Of Task)
        Return Task.CompletedTask
    End Function

    Private Async Function ShardConnectedAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " connected! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function ShardReadyAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " ready! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function MessageReceivedAsync(ByVal message As SocketMessage) As Task
        Dim content As String = message.Content
        Dim prefix As String = My.Settings.prefix
        Dim author As IUser = message.Author
        Dim member As SocketGuildUser = author

        If content.ToLower.StartsWith(prefix) Then
            If content.ToLower.StartsWith(prefix & "ping") Then
                Dim ping1 As String = _client.Shards(0).Latency
                ' Dim ping2 As String = _client.Shards(1).Latency
                Dim builder As EmbedBuilder = New EmbedBuilder
                builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                builder.WithThumbnailUrl(My.Settings.botIcon)
                builder.WithColor(219, 172, 69)

                builder.WithDescription("Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))

                Await message.Channel.SendMessageAsync("", False, builder.Build)

                Colorize("[INFO]      Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))
            ElseIf content.ToLower.StartsWith(prefix & "help") Then
                Dim split As String() = content.Split(" ")
                If split.Count > 1 Then
                    If split(1).ToLower = "settings" Then
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**Change bot prefix:**", "`" & prefix & "prefix [new prefix]`", True)
                        builder.AddField("**Change bot icon:**", "`" & prefix & "icon [image url]`", True)
                        builder.AddField("**Change bot name:**", "`" & prefix & "name [new name]`", True)
                        builder.AddField("**Check bot latency:**", "`" & prefix & "ping`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    ElseIf split(1).ToLower = "user" Then
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**View streams list:**", "`" & prefix & "streams list [page]`", True)
                        builder.AddField("**View random stream:**", "`" & prefix & "streams rnd`", True)
                        builder.AddField("**Add stream:**", "`" & prefix & "streams add [stream URL] [stream title]`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(219, 172, 69)

                    builder.WithDescription("Please use `" & prefix & "help` to see this menu any time." & Environment.NewLine & Environment.NewLine &
                                            "**Current Prefix:** `" & prefix & "`")

                    builder.AddField("**Settings Help:**", "`" & prefix & "help settings`", True)
                    builder.AddField("**User Help:**", "`" & prefix & "help user`", True)
                    builder.AddField("**Bot Developer:**", "OniSensei#0420", False)

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "prefix") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim newprefix As String() = content.Split(" ")
                    If newprefix.Count > 0 Then
                        My.Settings.prefix = newprefix(1)
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot prefix has been updated!")

                        builder.AddField("**Current prefix: **", "`" & My.Settings.prefix & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "icon") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim split As String() = content.Split(" ")
                    Dim newicon As String = split(1)
                    If newicon.Length > 0 Then
                        If newicon.ToLower.EndsWith(".png") Or newicon.ToLower.Contains(".jpg") Or newicon.ToLower.Contains(".jpeg") Or newicon.ToLower.Contains(".gif") Or newicon.ToLower.Contains(".bmp") Then
                            My.Settings.botIcon = newicon
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                            builder.WithThumbnailUrl(My.Settings.botIcon)
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot icon has been updated!")

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "name") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim split As String() = content.Split(" ")
                    Dim newbotName As String = content.Remove(0, prefix.Length + 5)
                    If newbotName.Length > 0 Then
                        My.Settings.botName = newbotName
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot name has been updated!")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                End If
            ElseIf content.ToLower.StartsWith(prefix & "streams") Then
                Dim split As String() = content.Split(" ")
                If split.Count > 0 Then
                    Dim sType As String = split(1)

                    Select Case sType
                        Case "list"
                            Dim page As Integer = Num(content)
                            Dim sbuilder As New StringBuilder
                            If page > 0 Then
                                sbuilder = LoadStreams(page)

                                Dim builder As EmbedBuilder = New EmbedBuilder
                                builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                builder.WithThumbnailUrl(My.Settings.botIcon)
                                builder.WithColor(219, 172, 69)

                                builder.WithDescription(sbuilder.ToString)

                                Await message.Channel.SendMessageAsync("", False, builder.Build)
                            End If
                        Case "rnd"
                            Dim rndStream As Integer = RandomNumber(1, StreamCount)
                            Dim streamURL As String = StreamQuery(rndStream, "streamURL")
                            Dim streamTitle As String = StreamQuery(rndStream, "streamTitle")
                            Dim streamAuthor As String = StreamQuery(rndStream, "streamUser")

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                            builder.WithThumbnailUrl(My.Settings.botIcon)
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("**[" & streamTitle & "](" & streamURL & ")** added by " & streamAuthor)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        Case "add"
                            If split.Count = 4 Then
                                If split(2).Contains("http") Then
                                    AddStream(split(2), split(3), author.Username)

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(219, 172, 69)

                                    builder.WithDescription("**[" & split(3) & "](" & split(2) & ")** added by " & author.Username)

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            ElseIf split.Count = 3 Then
                                If split(2).Contains("http") Then
                                    AddStream(split(2), "No Title", author.Username)

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(219, 172, 69)

                                    builder.WithDescription("**[No Title](" & split(2) & ")** added by " & author.Username)

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            End If
                        Case "wipe"
                            ClearStreams()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                            builder.WithThumbnailUrl(My.Settings.botIcon)
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription(author.Mention & "**Streams DB wiped.**")

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End Select
                End If
            End If
        End If
    End Function

    Private Function ConsoleEventCallback(ByVal eventType As Integer) As Boolean
        Select Case eventType
            Case 0
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 1
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 2
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 5
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 6
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
        End Select
        Return False
    End Function

    Dim handler As ConsoleEventDelegate
    Private Delegate Function ConsoleEventDelegate(ByVal eventType As Integer) As Boolean
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function SetConsoleCtrlHandler(ByVal callback As ConsoleEventDelegate, ByVal add As Boolean) As Boolean
    End Function
End Module