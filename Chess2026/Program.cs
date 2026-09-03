using System;

enum PieceType
{
    None,
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

enum Color
{
    None,
    White,
    Black
}

struct Piece
{
    public PieceType Type;
    public Color Color;

    public Piece(PieceType type, Color color)
    {
        Type = type;
        Color = color;
    }

    public override string ToString()
    {
        if (Type == PieceType.None)
            return ".";

        char c = Type switch
        {
            PieceType.Pawn => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => '?'
        };

        return Color == Color.White
            ? c.ToString()
            : char.ToLower(c).ToString();
    }
}

class ChessGame
{
    private readonly Piece[,] board = new Piece[8, 8];

    private Color turn = Color.White;

    private bool whiteKingMoved;
    private bool blackKingMoved;

    private bool whiteLeftRookMoved;
    private bool whiteRightRookMoved;
    private bool blackLeftRookMoved;
    private bool blackRightRookMoved;

    // Square available for en passant, or (-1, -1) if none.
    private int enPassantRow = -1;
    private int enPassantCol = -1;

    public ChessGame()
    {
        SetupBoard();
    }

    private void SetupBoard()
    {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = new Piece(PieceType.None, Color.None);

        // Black
        board[0, 0] = new Piece(PieceType.Rook, Color.Black);
        board[0, 1] = new Piece(PieceType.Knight, Color.Black);
        board[0, 2] = new Piece(PieceType.Bishop, Color.Black);
        board[0, 3] = new Piece(PieceType.Queen, Color.Black);
        board[0, 4] = new Piece(PieceType.King, Color.Black);
        board[0, 5] = new Piece(PieceType.Bishop, Color.Black);
        board[0, 6] = new Piece(PieceType.Knight, Color.Black);
        board[0, 7] = new Piece(PieceType.Rook, Color.Black);

        for (int c = 0; c < 8; c++)
            board[1, c] = new Piece(PieceType.Pawn, Color.Black);

        // White
        board[7, 0] = new Piece(PieceType.Rook, Color.White);
        board[7, 1] = new Piece(PieceType.Knight, Color.White);
        board[7, 2] = new Piece(PieceType.Bishop, Color.White);
        board[7, 3] = new Piece(PieceType.Queen, Color.White);
        board[7, 4] = new Piece(PieceType.King, Color.White);
        board[7, 5] = new Piece(PieceType.Bishop, Color.White);
        board[7, 6] = new Piece(PieceType.Knight, Color.White);
        board[7, 7] = new Piece(PieceType.Rook, Color.White);

        for (int c = 0; c < 8; c++)
            board[6, c] = new Piece(PieceType.Pawn, Color.White);
    }

    public void Run()
    {
        Console.WriteLine("=== C# CHESS ===");
        Console.WriteLine("Enter moves such as: e2 e4");
        Console.WriteLine("Castling: e1 g1 or e1 c1");
        Console.WriteLine("Type 'quit' to exit.");
        Console.WriteLine();

        while (true)
        {
            PrintBoard();

            if (IsCheckmate(turn))
            {
                Console.WriteLine($"{turn} is checkmated!");
                Console.WriteLine($"{Opponent(turn)} wins!");
                break;
            }

            if (IsStalemate(turn))
            {
                Console.WriteLine("Stalemate!");
                break;
            }

            if (IsInCheck(turn))
                Console.WriteLine($"{turn} is in check.");

            Console.Write($"{turn} to move: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            string[] parts = input
                .Trim()
                .Split(
                    new[] { ' ', '-', '>' },
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2 ||
                !TryParseSquare(parts[0], out int fromRow, out int fromCol) ||
                !TryParseSquare(parts[1], out int toRow, out int toCol))
            {
                Console.WriteLine("Invalid input. Example: e2 e4");
                continue;
            }

            if (!TryMove(fromRow, fromCol, toRow, toCol))
            {
                Console.WriteLine("Illegal move.");
                continue;
            }

            turn = Opponent(turn);
        }
    }

    private void PrintBoard()
    {
        Console.WriteLine();

        Console.WriteLine("    a b c d e f g h");
        Console.WriteLine("   -----------------");

        for (int r = 0; r < 8; r++)
        {
            Console.Write($"{8 - r} | ");

            for (int c = 0; c < 8; c++)
                Console.Write(board[r, c] + " ");

            Console.WriteLine($"| {8 - r}");
        }

        Console.WriteLine("   -----------------");
        Console.WriteLine("    a b c d e f g h");
        Console.WriteLine();
    }

    private bool TryMove(
        int fromRow,
        int fromCol,
        int toRow,
        int toCol)
    {
        if (!Inside(fromRow, fromCol) ||
            !Inside(toRow, toCol))
            return false;

        Piece piece = board[fromRow, fromCol];

        if (piece.Color != turn)
            return false;

        Piece destination = board[toRow, toCol];

        if (destination.Color == turn)
            return false;

        if (!IsPseudoLegalMove(
                fromRow, fromCol, toRow, toCol))
            return false;

        // Save state so we can undo the move if it exposes our king.
        Piece[,] backup = CopyBoard();

        int oldEnPassantRow = enPassantRow;
        int oldEnPassantCol = enPassantCol;

        bool oldWhiteKingMoved = whiteKingMoved;
        bool oldBlackKingMoved = blackKingMoved;

        bool oldWhiteLeftRookMoved = whiteLeftRookMoved;
        bool oldWhiteRightRookMoved = whiteRightRookMoved;
        bool oldBlackLeftRookMoved = blackLeftRookMoved;
        bool oldBlackRightRookMoved = blackRightRookMoved;

        ExecuteMove(fromRow, fromCol, toRow, toCol);

        if (IsInCheck(turn))
        {
            RestoreBoard(backup);

            enPassantRow = oldEnPassantRow;
            enPassantCol = oldEnPassantCol;

            whiteKingMoved = oldWhiteKingMoved;
            blackKingMoved = oldBlackKingMoved;

            whiteLeftRookMoved = oldWhiteLeftRookMoved;
            whiteRightRookMoved = oldWhiteRightRookMoved;
            blackLeftRookMoved = oldBlackLeftRookMoved;
            blackRightRookMoved = oldBlackRightRookMoved;

            return false;
        }

        // Pawn promotion.
        Piece moved = board[toRow, toCol];

        if (moved.Type == PieceType.Pawn &&
            (toRow == 0 || toRow == 7))
        {
            Promote(toRow, toCol);
        }

        return true;
    }

    private bool IsPseudoLegalMove(
        int fr,
        int fc,
        int tr,
        int tc)
    {
        Piece piece = board[fr, fc];

        int dr = tr - fr;
        int dc = tc - fc;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                return IsPawnMove(fr, fc, tr, tc);

            case PieceType.Knight:
                return
                    (Math.Abs(dr) == 2 && Math.Abs(dc) == 1) ||
                    (Math.Abs(dr) == 1 && Math.Abs(dc) == 2);

            case PieceType.Bishop:
                return
                    Math.Abs(dr) == Math.Abs(dc) &&
                    ClearPath(fr, fc, tr, tc);

            case PieceType.Rook:
                return
                    (dr == 0 || dc == 0) &&
                    ClearPath(fr, fc, tr, tc);

            case PieceType.Queen:
                return
                    ((dr == 0 || dc == 0) ||
                     Math.Abs(dr) == Math.Abs(dc)) &&
                    ClearPath(fr, fc, tr, tc);

            case PieceType.King:
                if (Math.Abs(dr) <= 1 &&
                    Math.Abs(dc) <= 1)
                    return true;

                return IsCastlingMove(fr, fc, tr, tc);

            default:
                return false;
        }
    }

    private bool IsPawnMove(
        int fr,
        int fc,
        int tr,
        int tc)
    {
        Piece pawn = board[fr, fc];

        int direction = pawn.Color == Color.White ? -1 : 1;
        int startRow = pawn.Color == Color.White ? 6 : 1;

        Piece target = board[tr, tc];

        // One square forward.
        if (tc == fc &&
            tr == fr + direction &&
            target.Type == PieceType.None)
        {
            return true;
        }

        // Two squares from starting position.
        if (tc == fc &&
            fr == startRow &&
            tr == fr + 2 * direction &&
            target.Type == PieceType.None &&
            board[fr + direction, fc].Type == PieceType.None)
        {
            return true;
        }

        // Normal capture.
        if (Math.Abs(tc - fc) == 1 &&
            tr == fr + direction &&
            target.Color == Opponent(pawn.Color))
        {
            return true;
        }

        // En passant.
        if (Math.Abs(tc - fc) == 1 &&
            tr == fr + direction &&
            target.Type == PieceType.None &&
            tr == enPassantRow &&
            tc == enPassantCol)
        {
            return true;
        }

        return false;
    }

    private bool IsCastlingMove(
        int fr,
        int fc,
        int tr,
        int tc)
    {
        if (fr != tr || Math.Abs(tc - fc) != 2)
            return false;

        Piece king = board[fr, fc];

        if (king.Type != PieceType.King)
            return false;

        if (IsInCheck(king.Color))
            return false;

        if (king.Color == Color.White)
        {
            if (whiteKingMoved)
                return false;

            // King-side: e1 -> g1
            if (tc == 6 &&
                !whiteRightRookMoved &&
                board[7, 7].Type == PieceType.Rook &&
                board[7, 7].Color == Color.White &&
                board[7, 5].Type == PieceType.None &&
                board[7, 6].Type == PieceType.None)
            {
                if (SquareAttacked(7, 5, Color.Black) ||
                    SquareAttacked(7, 6, Color.Black))
                    return false;

                return true;
            }

            // Queen-side: e1 -> c1
            if (tc == 2 &&
                !whiteLeftRookMoved &&
                board[7, 0].Type == PieceType.Rook &&
                board[7, 0].Color == Color.White &&
                board[7, 1].Type == PieceType.None &&
                board[7, 2].Type == PieceType.None &&
                board[7, 3].Type == PieceType.None)
            {
                if (SquareAttacked(7, 3, Color.Black) ||
                    SquareAttacked(7, 2, Color.Black))
                    return false;

                return true;
            }
        }
        else
        {
            if (blackKingMoved)
                return false;

            // King-side: e8 -> g8
            if (tc == 6 &&
                !blackRightRookMoved &&
                board[0, 7].Type == PieceType.Rook &&
                board[0, 7].Color == Color.Black &&
                board[0, 5].Type == PieceType.None &&
                board[0, 6].Type == PieceType.None)
            {
                if (SquareAttacked(0, 5, Color.White) ||
                    SquareAttacked(0, 6, Color.White))
                    return false;

                return true;
            }

            // Queen-side: e8 -> c8
            if (tc == 2 &&
                !blackLeftRookMoved &&
                board[0, 0].Type == PieceType.Rook &&
                board[0, 0].Color == Color.Black &&
                board[0, 1].Type == PieceType.None &&
                board[0, 2].Type == PieceType.None &&
                board[0, 3].Type == PieceType.None)
            {
                if (SquareAttacked(0, 3, Color.White) ||
                    SquareAttacked(0, 2, Color.White))
                    return false;

                return true;
            }
        }

        return false;
    }

    private void ExecuteMove(
        int fr,
        int fc,
        int tr,
        int tc)
    {
        Piece piece = board[fr, fc];

        // Reset en passant unless a new opportunity is created.
        enPassantRow = -1;
        enPassantCol = -1;

        // En passant capture.
        if (piece.Type == PieceType.Pawn &&
            fc != tc &&
            board[tr, tc].Type == PieceType.None)
        {
            board[fr, tc] =
                new Piece(PieceType.None, Color.None);
        }

        board[tr, tc] = piece;
        board[fr, fc] =
            new Piece(PieceType.None, Color.None);

        // Pawn moved two squares.
        if (piece.Type == PieceType.Pawn &&
            Math.Abs(tr - fr) == 2)
        {
            enPassantRow = (fr + tr) / 2;
            enPassantCol = fc;
        }

        // King movement.
        if (piece.Type == PieceType.King)
        {
            if (piece.Color == Color.White)
                whiteKingMoved = true;
            else
                blackKingMoved = true;

            // Castling: move rook too.
            if (Math.Abs(tc - fc) == 2)
            {
                if (tc == 6)
                {
                    board[tr, 5] = board[tr, 7];
                    board[tr, 7] =
                        new Piece(PieceType.None, Color.None);
                }
                else if (tc == 2)
                {
                    board[tr, 3] = board[tr, 0];
                    board[tr, 0] =
                        new Piece(PieceType.None, Color.None);
                }
            }
        }

        // Rook movement.
        if (piece.Type == PieceType.Rook)
            MarkRookMoved(piece.Color, fr, fc);

        // A rook captured on its original square also loses castling rights.
        Piece captured = board[tr, tc];

        if (captured.Type == PieceType.Rook)
            MarkRookMoved(captured.Color, tr, tc);
    }

    private void MarkRookMoved(
        Color color,
        int row,
        int col)
    {
        if (color == Color.White)
        {
            if (row == 7 && col == 0)
                whiteLeftRookMoved = true;

            if (row == 7 && col == 7)
                whiteRightRookMoved = true;
        }
        else if (color == Color.Black)
        {
            if (row == 0 && col == 0)
                blackLeftRookMoved = true;

            if (row == 0 && col == 7)
                blackRightRookMoved = true;
        }
    }

    private void Promote(int row, int col)
    {
        Console.WriteLine(
            $"{board[row, col].Color} pawn promotion.");

        while (true)
        {
            Console.Write("Choose Q, R, B, or N: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            switch (char.ToUpper(input[0]))
            {
                case 'Q':
                    board[row, col].Type = PieceType.Queen;
                    return;

                case 'R':
                    board[row, col].Type = PieceType.Rook;
                    return;

                case 'B':
                    board[row, col].Type = PieceType.Bishop;
                    return;

                case 'N':
                    board[row, col].Type = PieceType.Knight;
                    return;
            }

            Console.WriteLine("Invalid choice.");
        }
    }

    private bool IsInCheck(Color color)
    {
        FindKing(color, out int row, out int col);
        return SquareAttacked(row, col, Opponent(color));
    }

    private bool SquareAttacked(
        int row,
        int col,
        Color byColor)
    {
        // Pawns.
        int pawnDirection = byColor == Color.White ? -1 : 1;
        int pawnRow = row - pawnDirection;

        foreach (int dc in new[] { -1, 1 })
        {
            int pc = col + dc;

            if (Inside(pawnRow, pc) &&
                board[pawnRow, pc].Type == PieceType.Pawn &&
                board[pawnRow, pc].Color == byColor)
            {
                return true;
            }
        }

        // Knights.
        int[,] knightMoves =
        {
            { -2, -1 }, { -2, 1 },
            { -1, -2 }, { -1, 2 },
            { 1, -2 }, { 1, 2 },
            { 2, -1 }, { 2, 1 }
        };

        for (int i = 0; i < 8; i++)
        {
            int r = row + knightMoves[i, 0];
            int c = col + knightMoves[i, 1];

            if (Inside(r, c) &&
                board[r, c].Type == PieceType.Knight &&
                board[r, c].Color == byColor)
            {
                return true;
            }
        }

        // Sliding pieces.
        int[,] directions =
        {
            { -1, 0 },
            { 1, 0 },
            { 0, -1 },
            { 0, 1 },
            { -1, -1 },
            { -1, 1 },
            { 1, -1 },
            { 1, 1 }
        };

        for (int i = 0; i < 8; i++)
        {
            int r = row + directions[i, 0];
            int c = col + directions[i, 1];

            while (Inside(r, c))
            {
                Piece p = board[r, c];

                if (p.Type != PieceType.None)
                {
                    if (p.Color == byColor)
                    {
                        bool diagonal = i >= 4;

                        if ((!diagonal &&
                             (p.Type == PieceType.Rook ||
                              p.Type == PieceType.Queen)) ||
                            (diagonal &&
                             (p.Type == PieceType.Bishop ||
                              p.Type == PieceType.Queen)))
                        {
                            return true;
                        }
                    }

                    break;
                }

                r += directions[i, 0];
                c += directions[i, 1];
            }
        }

        // Enemy king.
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0)
                    continue;

                int r = row + dr;
                int c = col + dc;

                if (Inside(r, c) &&
                    board[r, c].Type == PieceType.King &&
                    board[r, c].Color == byColor)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasLegalMove(Color color)
    {
        for (int fr = 0; fr < 8; fr++)
        {
            for (int fc = 0; fc < 8; fc++)
            {
                if (board[fr, fc].Color != color)
                    continue;

                for (int tr = 0; tr < 8; tr++)
                {
                    for (int tc = 0; tc < 8; tc++)
                    {
                        if (fr == tr && fc == tc)
                            continue;

                        if (!IsPseudoLegalMove(fr, fc, tr, tc))
                            continue;

                        Piece[,] backup = CopyBoard();

                        int oldEnPassantRow = enPassantRow;
                        int oldEnPassantCol = enPassantCol;

                        bool oldWhiteKingMoved = whiteKingMoved;
                        bool oldBlackKingMoved = blackKingMoved;

                        bool oldWhiteLeftRookMoved =
                            whiteLeftRookMoved;

                        bool oldWhiteRightRookMoved =
                            whiteRightRookMoved;

                        bool oldBlackLeftRookMoved =
                            blackLeftRookMoved;

                        bool oldBlackRightRookMoved =
                            blackRightRookMoved;

                        ExecuteMove(fr, fc, tr, tc);

                        bool legal = !IsInCheck(color);

                        RestoreBoard(backup);

                        enPassantRow = oldEnPassantRow;
                        enPassantCol = oldEnPassantCol;

                        whiteKingMoved = oldWhiteKingMoved;
                        blackKingMoved = oldBlackKingMoved;

                        whiteLeftRookMoved =
                            oldWhiteLeftRookMoved;

                        whiteRightRookMoved =
                            oldWhiteRightRookMoved;

                        blackLeftRookMoved =
                            oldBlackLeftRookMoved;

                        blackRightRookMoved =
                            oldBlackRightRookMoved;

                        if (legal)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsCheckmate(Color color)
    {
        return IsInCheck(color) && !HasLegalMove(color);
    }

    private bool IsStalemate(Color color)
    {
        return !IsInCheck(color) && !HasLegalMove(color);
    }

    private bool ClearPath(
        int fr,
        int fc,
        int tr,
        int tc)
    {
        int dr = Math.Sign(tr - fr);
        int dc = Math.Sign(tc - fc);

        int r = fr + dr;
        int c = fc + dc;

        while (r != tr || c != tc)
        {
            if (board[r, c].Type != PieceType.None)
                return false;

            r += dr;
            c += dc;
        }

        return true;
    }

    private void FindKing(
        Color color,
        out int row,
        out int col)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (board[r, c].Type == PieceType.King &&
                    board[r, c].Color == color)
                {
                    row = r;
                    col = c;
                    return;
                }
            }
        }

        throw new InvalidOperationException(
            $"No {color} king exists.");
    }

    private Piece[,] CopyBoard()
    {
        Piece[,] copy = new Piece[8, 8];

        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                copy[r, c] = board[r, c];

        return copy;
    }

    private void RestoreBoard(Piece[,] copy)
    {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = copy[r, c];
    }

    private static Color Opponent(Color color)
    {
        return color switch
        {
            Color.White => Color.Black,
            Color.Black => Color.White,
            _ => Color.None
        };
    }

    private static bool Inside(int row, int col)
    {
        return row >= 0 && row < 8 &&
               col >= 0 && col < 8;
    }

    private static bool TryParseSquare(
        string square,
        out int row,
        out int col)
    {
        row = -1;
        col = -1;

        if (square.Length != 2)
            return false;

        char file = char.ToLower(square[0]);
        char rank = square[1];

        if (file < 'a' || file > 'h' ||
            rank < '1' || rank > '8')
            return false;

        col = file - 'a';
        row = 8 - (rank - '0');

        return true;
    }
}

class Program
{
    static void Main()
    {
        ChessGame game = new ChessGame();
        game.Run();
    }
}