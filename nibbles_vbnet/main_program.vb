' Copyright Wing-chung Leung 2017, except for level design and sound,
' which is owned by Microsoft. All rights reserved.
' Use of this source code is governed by a GPL-v3 license that can be found in
' the LICENSE file.

Option Strict On

Imports Point = System.Drawing.Point
Imports Size = System.Drawing.Size

Module main_program

    Enum FieldObject
        Background
        Snake1
        Snake2
        Wall
    End Enum

    Enum Direction
        Null
        Up
        Right
        South
        Left
    End Enum

    Enum LevelSelection
        GoToLevelOne
        RemainInSameLevel
        GoToNextLevel
    End Enum

    Structure GameConfig
        Dim numPlayers As Integer
        Dim gameSpeed As Integer
        Dim increaseSpeedDuringPlay As Boolean
    End Structure

    Structure SnakeData
        Dim Body As Queue(Of Point)
        Dim TargetLength As Integer
        Dim Position As Point
        Dim Directions As List(Of Direction)
        Dim LivesLeft As Integer
        Dim Score As Integer
        Dim IsAlive As Boolean
    End Structure

    Structure GameColors
        Const Background As ConsoleColor = ConsoleColor.DarkBlue
        Const Snake1 As ConsoleColor = ConsoleColor.Yellow
        Const Snake2 As ConsoleColor = ConsoleColor.Magenta
        Const Wall As ConsoleColor = ConsoleColor.Red
        Const Score As ConsoleColor = ConsoleColor.White
        Const DialogForeground As ConsoleColor = ConsoleColor.White
        Const DialogBackground As ConsoleColor = ConsoleColor.DarkRed
        Const Number As ConsoleColor = ConsoleColor.Yellow
        Const Sparkle As ConsoleColor = ConsoleColor.DarkMagenta
    End Structure

    Const maxNumberInLevel As Integer = 9
    Const maxGameSpeed As Integer = 100
    Const speedIncreasePerNumber As Integer = 2
    Const numberOfLevels As Integer = 10
    Const livesInGame As Integer = 5

    Const scoreMultiplierOfNumbers As Integer = 100
    Const lengthMultiplierOfNumbers As Integer = 4
    Const scorePenaltyForCrash As Integer = 1000

    Const screenWidth As Integer = 80
    Const screenHeight As Integer = 25

    ReadOnly colorTable() As ConsoleColor = {
        GameColors.Background,
        GameColors.Snake1,
        GameColors.Snake2,
        GameColors.Wall
    }

    Dim gameField(80 - 1, 50 - 1) As FieldObject
    Dim currentFrameInterval As TimeSpan
    Dim timeForNextFrame As Date

    Function InitializeConsole() As Boolean
        Shell("cmd.exe /c chcp 437",, True)
        Console.OutputEncoding = Text.Encoding.UTF8

        Try
            ' The following commands can fail in a console emulator. In case
            ' of failure just skip the failed commands.
            Console.SetWindowSize(screenWidth, screenHeight)
            Console.SetBufferSize(screenWidth, screenHeight)
        Catch ex As Exception
        End Try

        If Console.WindowWidth < screenWidth OrElse
           Console.WindowHeight < screenHeight Then
            Console.WriteLine(
                "The console should be at least {0} x {1} in size.",
                screenWidth, screenHeight)
            Return False
        End If
        ScreenUtilities.DisableConsoleResizing()

        Return True
    End Function

    Sub ResetConsole()
        Console.CursorVisible = True
        Console.ResetColor()
        Console.Clear()
    End Sub

    Sub DisplayText(left As Integer, top As Integer, text As String,
            Optional foregroundColor As ConsoleColor = ConsoleColor.Gray,
            Optional backgroundColor As ConsoleColor = ConsoleColor.Black)
        Console.ForegroundColor = foregroundColor
        Console.BackgroundColor = backgroundColor
        Try
            Console.SetCursorPosition(left, top)
            Console.Write(text)
        Catch ex As Exception
        End Try
    End Sub

    Sub DisplayTextCentered(top As Integer, text As String,
            Optional foregroundColor As ConsoleColor = ConsoleColor.Gray,
            Optional backgroundColor As ConsoleColor = ConsoleColor.Black)
        DisplayText((screenWidth - Len(text)) \ 2, top, text,
                    foregroundColor, backgroundColor)
    End Sub

    Sub ClearKeyboardBuffer()
        Do While Console.KeyAvailable
            Console.ReadKey(True)
        Loop
    End Sub

    ' Shows a rotating sparkle patten on the screen which ends with a key press.
    Sub PauseWithRotatingSparkle(left As Integer, top As Integer,
                                 width As Integer, height As Integer,
                                 interval As Integer,
                                 Optional sparkleChar As Char = "*"c)
        Const frameDuration = 50  ' unit = millisecond

        Console.CursorVisible = False
        ClearKeyboardBuffer()

        Dim sparklePattern As String = sparkleChar + Space(interval - 1)
        Do While Len(sparklePattern) < width + interval - 1
            sparklePattern &= sparklePattern
        Loop

        Dim frameNumber As Integer = 0
        Do Until Console.KeyAvailable
            ' Print horizontal sparkles.
            DisplayText(left, top,
                Mid(sparklePattern, frameNumber Mod interval + 1, width),
                GameColors.Sparkle)
            DisplayText(left, top + height - 1,
                Mid(sparklePattern,
                    interval -
                        (width * 2 + height - 3 + frameNumber - 1) Mod interval,
                    width),
                GameColors.Sparkle)

            'Print Vertical sparkles
            For i As Integer = 1 To height - 2
                Dim sparklePosition = width + i - 1 + frameNumber
                DisplayText(left + width - 1, top + i,
                    If(sparklePosition Mod interval = 0, sparkleChar, " "),
                    GameColors.Sparkle)

                sparklePosition = width * 2 + height * 2 - 4 - i + frameNumber
                DisplayText(left, top + i,
                    If(sparklePosition Mod interval = 0, sparkleChar, " "),
                    GameColors.Sparkle)
            Next
            frameNumber = (frameNumber + 1) Mod interval

            System.Threading.Thread.Sleep(frameDuration)
        Loop
        ClearKeyboardBuffer()
    End Sub

    Sub ShowGameIntro()
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()
        DisplayTextCentered(2, "V B . N E T   N I B B L E S", ConsoleColor.White)

        DisplayTextCentered(4, "A remake of QBasic Nibbles (1991) in VB.NET")
        DisplayTextCentered(5, "Copyright (C) Wing-chung Leung 2017")
        DisplayTextCentered(7, "Note: QBasic Nibbles is a property of Microsoft Corporation")

        DisplayTextCentered(9, "Navigate your snakes to eat numbers.  When you eat a number,")
        DisplayTextCentered(10, "you gain points and your snake becomes longer. Avoid running into")
        DisplayTextCentered(11, "anything else, i.e. walls, your snake, and the other snake.")

        DisplayTextCentered(13, "G A M E   C O N T R O L S", ConsoleColor.White)

        DisplayTextCentered(15, " General             Player 1          Player 2 (Optional) ")
        DisplayTextCentered(16, "                       (Up)                   (Up)         ")
        DisplayTextCentered(17, "P - Pause                ↑                      W          ")
        DisplayTextCentered(18, "                (Left) ←   → (Right)   (Left) A   D (Right)")
        DisplayTextCentered(19, "                         ↓                      S          ")
        DisplayTextCentered(20, "                      (Down)                 (Down)        ")

        DisplayTextCentered(23, "Press any key to continue")

        Call New System.Media.SoundPlayer(My.Resources.SoundStartGame).Play()
        PauseWithRotatingSparkle(0, 0, 80, 22, 5)
    End Sub

    Function GetGameConfig() As GameConfig
        Console.CursorVisible = True
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Dim config As GameConfig

        DisplayText(19, 3, "How many players? (1 or 2) ")
        Do
            Select Case Console.ReadKey(True).Key
                Case ConsoleKey.D1, ConsoleKey.NumPad1
                    Console.Write("1")
                    config.numPlayers = 1
                    Exit Do
                Case ConsoleKey.D2, ConsoleKey.NumPad2
                    Console.Write("2")
                    config.numPlayers = 2
                    Exit Do
            End Select
        Loop

        DisplayText(20, 6, "Skill level? (1 to 100) ")
        DisplayText(21, 7, "1   = Novice")
        DisplayText(21, 8, "90  = Expert")
        DisplayText(21, 9, "100 = Twiddle Fingers")
        DisplayText(14, 10, "(Computer speed may affect your skill level)")
        Do
            DisplayText(44, 6, Space(35))
            Console.CursorLeft = 44
        Loop Until _
            Integer.TryParse(Console.ReadLine(), config.gameSpeed) AndAlso
            config.gameSpeed >= 1 AndAlso config.gameSpeed <= 100

        DisplayText(14, 13, "Increase game speed during play? (Y or N) ")
        Do
            Select Case Console.ReadKey(True).Key
                Case ConsoleKey.Y
                    Console.Write("Y")
                    config.increaseSpeedDuringPlay = True
                    Exit Do
                Case ConsoleKey.N
                    Console.Write("N")
                    config.increaseSpeedDuringPlay = False
                    Exit Do
            End Select
        Loop

        DisplayText(19, 16, "▀▄▀▄▀▄ Console font settings ▀▄▀▄▀▄")
        DisplayText(14, 17, "Please check if the pattern above are squares.")
        DisplayText(14, 18, "If not, right click the title bar, choose ""Properties"",")
        DisplayText(14, 19, "and in the font tab change the font to ""Consolas"".")

        DisplayText(14, 21, "Press any key to continue...")

        ClearKeyboardBuffer()
        Console.ReadKey(True)
        ClearKeyboardBuffer()

        Return config
    End Function

    Sub DisplayPointInField(left As Integer, top As Integer)
        Dim screenTop As Integer = top \ 2

        If left = Console.WindowWidth - 1 AndAlso
           screenTop = Console.WindowHeight - 1 Then
            ' This is a hack to avoid the glitch in Windows 7 writing at the
            ' lower-right corner. This depends on the fact that the top-right
            ' corner of the field is the same as the lower-right corner.
            Console.MoveBufferArea(left, 1, 1, 1, left, screenTop)
            screenTop = 1
        End If

        ' Shows the background object in the background color to minimize the
        ' one-pixel glitch for the half-block characters.
        If gameField(left, screenTop * 2) = FieldObject.Background Then
            DisplayText(left, screenTop, "▄",
                        colorTable(gameField(left, screenTop * 2 + 1)),
                        colorTable(gameField(left, screenTop * 2)))
        Else
            DisplayText(left, screenTop, "▀",
                        colorTable(gameField(left, screenTop * 2)),
                        colorTable(gameField(left, screenTop * 2 + 1)))
        End If
    End Sub

    Sub SetPointInField(left As Integer, top As Integer, obj As FieldObject)
        If gameField(left, top) = obj Then Return

        gameField(left, top) = obj
        DisplayPointInField(left, top)
    End Sub

    Sub AddWallToField(left As Integer, top As Integer)
        SetPointInField(left, top, FieldObject.Wall)
    End Sub

    Sub ClearField()
        Console.BackgroundColor = GameColors.Background
        Console.Clear()

        For top As Integer = 0 To 49
            For left As Integer = 0 To 79
                gameField(left, top) = FieldObject.Background
            Next
        Next
    End Sub

    ' Shows a dialog and wait for one of the specified keys, or any key if no
    ' key is specified. The text is center-aligned in the dialog.
    Function ShowDialog(text As String,
                        keysToContinue() As ConsoleKey) As ConsoleKeyInfo
        Dim messages As String() =
            text.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None)

        Dim maxLength As Integer = 0
        For Each line In messages
            maxLength = Math.Max(maxLength, Len(line))
        Next

        Dim screenTopOfDialog = (22 - messages.Count) \ 2
        Dim leftOfDialog = (screenWidth - maxLength - 4) \ 2

        Dim top = screenTopOfDialog
        DisplayText(leftOfDialog, top, "█" & StrDup(maxLength + 2, "▀"c) & "█",
                    GameColors.DialogForeground, GameColors.DialogBackground)
        top += 1

        For Each line In messages
            Dim length As Integer = Len(line)
            Dim centeredLine As String = Space((maxLength - length) \ 2) &
                                         Left(line, maxLength) &
                                         Space((maxLength - length + 1) \ 2)
            DisplayText(leftOfDialog, top, "█ " & centeredLine & " █",
                        GameColors.DialogForeground,
                        GameColors.DialogBackground)
            top += 1
        Next

        DisplayText(leftOfDialog, top, "█" & StrDup(maxLength + 2, "▄"c) & "█",
                    GameColors.DialogForeground, GameColors.DialogBackground)

        ClearKeyboardBuffer()
        Dim key As ConsoleKeyInfo
        Do
            key = Console.ReadKey(True)
        Loop Until keysToContinue.Count = 0 OrElse
                   keysToContinue.Contains(key.Key)
        ClearKeyboardBuffer()

        ' Restore the screen background
        For i As Integer = 0 To messages.Count + 1
            top = (screenTopOfDialog + i) * 2
            For j As Integer = 0 To maxLength + 3
                DisplayPointInField(leftOfDialog + j, top)
            Next
        Next

        Return key
    End Function

    Sub ShowLevelIntro(level As Integer)
        ShowDialog(String.Format("Level {0},  Push Space", level),
                   {ConsoleKey.Spacebar})
    End Sub

    Sub PrintLivesAndScore(numPlayers As Integer, snakes() As SnakeData)
        DisplayText(48, 0,
                    String.Format("SAMMY-->  Lives: {0} {1,13:N0}",
                                  snakes(0).LivesLeft, snakes(0).Score),
                    GameColors.Score, GameColors.Background)

        If numPlayers < 2 Then Return
        DisplayText(0, 0,
                    String.Format("{1,9:N0}  Lives: {0}  <--Jake",
                                  snakes(1).LivesLeft, snakes(1).Score),
                    GameColors.Score, GameColors.Background)
    End Sub

    Sub InitializeLevel(level As Integer, numPlayers As Integer,
                        snakes() As SnakeData)
        For i As Integer = 0 To 1
            snakes(i).IsAlive = True
            snakes(i).TargetLength = 2
            snakes(i).Body = New Queue(Of Point)
            snakes(i).Directions = New List(Of Direction)
        Next

        ClearField()

        ' Create outside border
        For left As Integer = 0 To 79
            AddWallToField(left, 2)
            AddWallToField(left, 49)
        Next
        For top As Integer = 3 To 48
            AddWallToField(0, top)
            AddWallToField(79, top)
        Next

        ' Create interior of a level, and specify the positions and directions
        ' of the snakes.
        Select Case level
            Case 1
                snakes(0).Position = New Point(49, 25)
                snakes(0).Directions.Add(Direction.Right)
                snakes(1).Position = New Point(30, 25)
                snakes(1).Directions.Add(Direction.Left)
            Case 2
                For left As Integer = 19 To 59
                    AddWallToField(left, 24)
                Next
                snakes(0).Position = New Point(59, 8)
                snakes(0).Directions.Add(Direction.Left)
                snakes(1).Position = New Point(20, 42)
                snakes(1).Directions.Add(Direction.Right)
            Case 3
                For top As Integer = 9 To 39
                    AddWallToField(20, top)
                    AddWallToField(59, top)
                Next
                snakes(0).Position = New Point(49, 26)
                snakes(0).Directions.Add(Direction.Up)
                snakes(1).Position = New Point(30, 25)
                snakes(1).Directions.Add(Direction.South)
            Case 4
                For i As Integer = 3 To 29
                    AddWallToField(20, i)
                    AddWallToField(59, 51 - i)
                Next
                For i As Integer = 1 To 39
                    AddWallToField(i, 37)
                    AddWallToField(79 - i, 14)
                Next
                snakes(0).Position = New Point(59, 8)
                snakes(0).Directions.Add(Direction.Left)
                snakes(1).Position = New Point(20, 42)
                snakes(1).Directions.Add(Direction.Right)
            Case 5
                For left As Integer = 22 To 56
                    AddWallToField(left, 10)
                    AddWallToField(left, 41)
                Next
                For top As Integer = 12 To 39
                    AddWallToField(20, top)
                    AddWallToField(58, top)
                Next
                snakes(0).Position = New Point(49, 26)
                snakes(0).Directions.Add(Direction.Up)
                snakes(1).Position = New Point(29, 25)
                snakes(1).Directions.Add(Direction.South)
            Case 6
                For top As Integer = 3 To 48
                    If top >= 22 AndAlso top <= 29 Then Continue For
                    For left As Integer = 9 To 69 Step 10
                        AddWallToField(left, top)
                    Next
                Next top
                snakes(0).Position = New Point(64, 6)
                snakes(0).Directions.Add(Direction.South)
                snakes(1).Position = New Point(14, 45)
                snakes(1).Directions.Add(Direction.Up)
            Case 7
                For top As Integer = 4 To 49 Step 2
                    AddWallToField(39, top)
                Next
                snakes(0).Position = New Point(64, 6)
                snakes(0).Directions.Add(Direction.South)
                snakes(1).Position = New Point(14, 45)
                snakes(1).Directions.Add(Direction.Up)
            Case 8
                For i As Integer = 3 To 39
                    AddWallToField(9, i)
                    AddWallToField(19, 51 - i)
                    AddWallToField(29, i)
                    AddWallToField(39, 51 - i)
                    AddWallToField(49, i)
                    AddWallToField(59, 51 - i)
                    AddWallToField(69, i)
                Next
                snakes(0).Position = New Point(64, 6)
                snakes(0).Directions.Add(Direction.South)
                snakes(1).Position = New Point(14, 45)
                snakes(1).Directions.Add(Direction.Up)
            Case 9
                For i As Integer = 5 To 46
                    AddWallToField(i, i)
                    AddWallToField(i + 28, i)
                Next
                snakes(0).Position = New Point(74, 39)
                snakes(0).Directions.Add(Direction.Up)
                snakes(1).Position = New Point(5, 12)
                snakes(1).Directions.Add(Direction.South)
            Case Else
                For i As Integer = 3 To 48 Step 2
                    AddWallToField(9, i)
                    AddWallToField(19, i + 1)
                    AddWallToField(29, i)
                    AddWallToField(39, i + 1)
                    AddWallToField(49, i)
                    AddWallToField(59, i + 1)
                    AddWallToField(69, i)
                Next
                snakes(0).Position = New Point(64, 6)
                snakes(0).Directions.Add(Direction.South)
                snakes(1).Position = New Point(14, 45)
                snakes(1).Directions.Add(Direction.Up)
        End Select

        ' Disable the second snake in one-player mode.
        If numPlayers = 1 Then
            snakes(1).Position = New Point(-1, -1)
        End If
    End Sub

    Sub SetGameSpeed(speed As Integer, isRelative As Boolean)
        Static currentGameSpeed As Integer = 1
        currentGameSpeed = Math.Min(
            If(isRelative, currentGameSpeed + speed, speed), maxGameSpeed)
        currentFrameInterval = New TimeSpan(0, 0, 0, 0, 121 - currentGameSpeed)
    End Sub

    Sub SetTimeForNextFrame(forcedReset As Boolean)
        Dim now As Date = Date.Now()
        If forcedReset Then
            timeForNextFrame = now + currentFrameInterval
        Else
            timeForNextFrame += currentFrameInterval
            ' Reset time if lagged too much, which implies the game
            ' process has been paused.
            If timeForNextFrame < now Then
                timeForNextFrame = now + currentFrameInterval
            End If
        End If
    End Sub

    Sub InitializeRound(level As Integer, showIntro As Boolean,
                        numPlayers As Integer, snakes() As SnakeData)
        InitializeLevel(level, numPlayers, snakes)
        PrintLivesAndScore(numPlayers, snakes)
        If showIntro Then
            ShowLevelIntro(level)
        End If
        SetTimeForNextFrame(True)
    End Sub

    ' Add new directions to the snakes according to user input, if the new
    ' directions do not move a snake backward. Each direction in the queue lasts
    ' for one move except for the last inputted direction.
    Sub InputSnakeDirections(numPlayers As Integer, snakes() As SnakeData)
        Do While Console.KeyAvailable
            Dim key As ConsoleKey = Console.ReadKey(True).Key
            Select Case key
                Case ConsoleKey.UpArrow, ConsoleKey.NumPad8
                    If snakes(0).Directions.Last <> Direction.South Then
                        snakes(0).Directions.Add(Direction.Up)
                    End If
                Case ConsoleKey.LeftArrow, ConsoleKey.NumPad4
                    If snakes(0).Directions.Last <> Direction.Right Then
                        snakes(0).Directions.Add(Direction.Left)
                    End If
                Case ConsoleKey.DownArrow, ConsoleKey.NumPad2
                    If snakes(0).Directions.Last <> Direction.Up Then
                        snakes(0).Directions.Add(Direction.South)
                    End If
                Case ConsoleKey.RightArrow, ConsoleKey.NumPad6
                    If snakes(0).Directions.Last <> Direction.Left Then
                        snakes(0).Directions.Add(Direction.Right)
                    End If
                Case ConsoleKey.P
                    ShowDialog("Game Paused... Push Space",
                               {ConsoleKey.Spacebar})
                    SetTimeForNextFrame(True)
            End Select

            If numPlayers = 1 Then Continue Do

            Select Case key
                Case ConsoleKey.W
                    If snakes(1).Directions.Last <> Direction.South Then
                        snakes(1).Directions.Add(Direction.Up)
                    End If
                Case ConsoleKey.A
                    If snakes(1).Directions.Last <> Direction.Right Then
                        snakes(1).Directions.Add(Direction.Left)
                    End If
                Case ConsoleKey.S
                    If snakes(1).Directions.Last <> Direction.Up Then
                        snakes(1).Directions.Add(Direction.South)
                    End If
                Case ConsoleKey.D
                    If snakes(1).Directions.Last <> Direction.Left Then
                        snakes(1).Directions.Add(Direction.Right)
                    End If
            End Select
        Loop
    End Sub

    ' Move both snake heads according to the directions of the snakes. If more
    ' than one direction is stored, remove the first one as it expires.
    Sub MoveSnakeHeads(numPlayers As Integer, snakes() As SnakeData)
        For i As Integer = 0 To numPlayers - 1
            If snakes(i).Directions.Count > 1 Then
                snakes(i).Directions.RemoveAt(0)
            End If
            Select Case snakes(i).Directions.First
                Case Direction.Up
                    snakes(i).Position += New Size(0, -1)
                Case Direction.Right
                    snakes(i).Position += New Size(1, 0)
                Case Direction.South
                    snakes(i).Position += New Size(0, 1)
                Case Else
                    snakes(i).Position += New Size(-1, 0)
            End Select
        Next
    End Sub

    ' Kill both snakes if the heads collide. Returns whether the snakes are
    ' killed.
    Function KillSnakesIfHeadsCollide(numPlayers As Integer,
                                      snakes() As SnakeData) As Boolean
        If numPlayers = 2 AndAlso snakes(0).Position = snakes(1).Position Then
            snakes(0).IsAlive = False
            snakes(1).IsAlive = False
            Return True
        End If
        Return False
    End Function

    ' Generate the specified number, display it on the screen, and returns its
    ' position.
    Sub GenerateNumber(number As Integer, ByRef left As Integer,
                       ByRef screenTop As Integer)
        Do
            left = CInt(Int(Rnd() * 78)) + 1
            screenTop = CInt(Int(Rnd() * 22)) + 2
        Loop Until gameField(left, screenTop * 2) = FieldObject.Background _
            AndAlso gameField(left, screenTop * 2 + 1) = FieldObject.Background

        DisplayText(left, screenTop, number.ToString(),
                    GameColors.Number, GameColors.Background)
    End Sub

    ' Check if the snakes hits the numbers. If yes add to score, increase
    ' length of snake, increase speed (if appropriate) and play sound. If both
    ' snakes hit the number, decide who gets the number randomly.
    Sub ScoreIfHitNumber(config As GameConfig, snakes() As SnakeData,
                         ByRef numberScreenTop As Integer,
                         ByRef numberLeft As Integer,
                         ByRef nextNumberToHit As Integer,
                         ByRef isRoundWon As Boolean)
        Dim snakeHittingNumber As Integer = -1
        For i = 0 To config.numPlayers - 1
            If snakes(i).Position.X = numberLeft AndAlso
               snakes(i).Position.Y \ 2 = numberScreenTop AndAlso
               (i = 0 OrElse snakeHittingNumber <> 0 OrElse Rnd() < 0.5F) Then
                snakeHittingNumber = i
            End If
        Next

        If snakeHittingNumber >= 0 Then
            snakes(snakeHittingNumber).TargetLength +=
                nextNumberToHit * lengthMultiplierOfNumbers
            snakes(snakeHittingNumber).Score +=
                nextNumberToHit * scoreMultiplierOfNumbers
            PrintLivesAndScore(config.numPlayers, snakes)

            If config.increaseSpeedDuringPlay Then
                SetGameSpeed(speedIncreasePerNumber, True)
            End If

            nextNumberToHit += 1
            If nextNumberToHit > maxNumberInLevel Then
                isRoundWon = True
            Else
                GenerateNumber(nextNumberToHit, numberLeft, numberScreenTop)
            End If

            Call New System.Media.SoundPlayer(My.Resources.SoundHitNumber).
                                  Play()
        End If
    End Sub

    Sub MoveSnakeBodiesOrKillSnake(numPlayers As Integer, snakes() As SnakeData)
        ' Erase trails if already at target length.
        For i = 0 To numPlayers - 1
            If snakes(i).Body.Count >= snakes(i).TargetLength Then
                Dim pointToRemove As Point = snakes(i).Body.Dequeue()
                SetPointInField(pointToRemove.X, pointToRemove.Y,
                                FieldObject.Background)
            End If
        Next

        ' Add snake head to the body, or kill it because of crash.
        For i As Integer = 0 To numPlayers - 1
            If gameField(snakes(i).Position.X, snakes(i).Position.Y) <>
                   FieldObject.Background Then
                snakes(i).IsAlive = False
            Else
                snakes(i).Body.Enqueue(snakes(i).Position)
                SetPointInField(
                    snakes(i).Position.X, snakes(i).Position.Y,
                    If(i = 0, FieldObject.Snake1, FieldObject.Snake2))
            End If
        Next
    End Sub

    ' Show the animation for erasing the snakes.
    Sub EraseSnakes(numPlayers As Integer, snakes() As SnakeData)
        Const animationSteps As Integer = 10
        Const animationFrameTime As Integer = 30  ' unit = millisecond

        Dim snakeBody(numPlayers - 1)() As Point
        Dim snakeIndex(numPlayers - 1) As Integer
        For i As Integer = 0 To numPlayers - 1
            snakeBody(i) = snakes(i).Body.ToArray()
            snakeIndex(i) = snakeBody(i).Count
        Next

        Dim actualSteps As Integer = Math.Min(animationSteps, snakeIndex.Max())
        For frameNumber As Integer = 0 To actualSteps - 1
            For i As Integer = 0 To numPlayers - 1
                snakeIndex(i) -= 1
                For j As Integer = snakeIndex(i) To 0 Step -animationSteps
                    SetPointInField(snakeBody(i)(j).X, snakeBody(i)(j).Y,
                                    FieldObject.Background)
                Next
            Next
            System.Threading.Thread.Sleep(animationFrameTime)
        Next
    End Sub

    ' Plays a round of nibbles. Returns if the round is won. The snakes' lives
    ' and score may also change.
    Function PlayRound(config As GameConfig, level As Integer,
                       showIntro As Boolean, snakes() As SnakeData) As Boolean
        InitializeRound(level, showIntro, config.numPlayers, snakes)
        Call New System.Media.SoundPlayer(My.Resources.SoundStartRound).Play()

        Dim isRoundWon As Boolean = False
        Dim numberScreenTop As Integer
        Dim numberLeft As Integer
        Dim nextNumberToHit As Integer = 1
        GenerateNumber(nextNumberToHit, numberLeft, numberScreenTop)
        Do
            System.Threading.Thread.Sleep(0)

            InputSnakeDirections(config.numPlayers, snakes)

            If Date.Now < timeForNextFrame Then
                Continue Do
            Else
                SetTimeForNextFrame(False)
            End If

            MoveSnakeHeads(config.numPlayers, snakes)

            ' Check if the snakes' heads collide. In this case both snakes
            ' die, the snakes do not score for hitting a number and the
            ' level does not advance.
            If KillSnakesIfHeadsCollide(config.numPlayers, snakes) Then
                Exit Do
            End If

            ScoreIfHitNumber(config, snakes, numberScreenTop, numberLeft,
                             nextNumberToHit, isRoundWon)

            ' Move the snakes even if the level is already won. The other snake
            ' can be killed at the same time.
            MoveSnakeBodiesOrKillSnake(config.numPlayers, snakes)
        Loop While Not isRoundWon AndAlso
                   snakes(0).IsAlive AndAlso snakes(1).IsAlive

        For i = 0 To config.numPlayers - 1
            If Not snakes(i).IsAlive Then
                snakes(i).Score -= scorePenaltyForCrash
                snakes(i).LivesLeft -= 1
                SetGameSpeed(config.gameSpeed, False)
            End If
        Next

        PrintLivesAndScore(config.numPlayers, snakes)
        If Not snakes(0).IsAlive OrElse Not snakes(1).IsAlive Then
            Call New System.Media.SoundPlayer(My.Resources.SoundSnakeDie).Play()
        End If
        EraseSnakes(config.numPlayers, snakes)
        Return isRoundWon
    End Function

    ' Play nibbles until a player running out of lives.
    Sub PlayGame(config As GameConfig)
        Console.CursorVisible = False

        Dim snakes(2 - 1) As SnakeData
        For i As Integer = 0 To 1
            snakes(i).LivesLeft = livesInGame
            snakes(i).Score = 0
        Next

        Dim currentLevel As Integer = 1
        Dim showIntro As Boolean = True
        SetGameSpeed(config.gameSpeed, False)
        Do
            Dim isRoundWon As Boolean = PlayRound(config, currentLevel,
                                                  showIntro, snakes)
            If isRoundWon Then
                currentLevel += 1
            End If

            ' If the round is won, show the introduction of the new level.
            ' Otherwise there is a dialog for losing a life.
            showIntro = isRoundWon

            ' It is possible to have one snake dying and the other advancing to
            ' the next level at the same time. In this case both dialogs are
            ' shown if the lives are not used up.
            If Not snakes(0).IsAlive OrElse Not snakes(1).IsAlive Then
                Dim message As String =
                    If(snakes(0).IsAlive, "<--- Jake Died! Push Space!",
                       If(snakes(1).IsAlive, "Sammy Died! Push Space! --->",
                                             "<-- Both Died! Push Space! -->"))
                ShowDialog(message, {ConsoleKey.Spacebar})
            End If
        Loop While snakes(0).LivesLeft > 0 AndAlso snakes(1).LivesLeft > 0
    End Sub

    Function StillWantsToPlay() As Boolean
        Dim key As ConsoleKeyInfo = ShowDialog(
            "G A M E   O V E R" & vbCrLf & vbCrLf & "Play Again? (Y/N)",
            {ConsoleKey.Y, ConsoleKey.N})
        Return key.Key = ConsoleKey.Y
    End Function

    Sub Main()
        If Not InitializeConsole() Then Return

        ShowGameIntro()
        Dim config As GameConfig = GetGameConfig()
        Do
            PlayGame(config)
        Loop While StillWantsToPlay()

        ResetConsole()
    End Sub

End Module
