import tkinter as tk
from tkinter import ttk
import ast
import math
import operator
import re


# ============================================================
# Scientific Calculator
# Tkinter GUI | DEG/RAD | Memory | History | Keyboard support
# ============================================================

# -------------------- Safe expression engine -----------------

BINARY_OPS = {
    ast.Add: operator.add,
    ast.Sub: operator.sub,
    ast.Mult: operator.mul,
    ast.Div: operator.truediv,
    ast.Pow: operator.pow,
    ast.Mod: operator.mod,
    ast.FloorDiv: operator.floordiv,
}

UNARY_OPS = {
    ast.UAdd: operator.pos,
    ast.USub: operator.neg,
}


class Calculator:
    def __init__(self):
        self.angle_mode = "DEG"
        self.memory = 0
        self.ans = 0
        self.history = []

    def _sin(self, x):
        return math.sin(math.radians(x)) if self.angle_mode == "DEG" else math.sin(x)

    def _cos(self, x):
        return math.cos(math.radians(x)) if self.angle_mode == "DEG" else math.cos(x)

    def _tan(self, x):
        return math.tan(math.radians(x)) if self.angle_mode == "DEG" else math.tan(x)

    def _asin(self, x):
        value = math.asin(x)
        return math.degrees(value) if self.angle_mode == "DEG" else value

    def _acos(self, x):
        value = math.acos(x)
        return math.degrees(value) if self.angle_mode == "DEG" else value

    def _atan(self, x):
        value = math.atan(x)
        return math.degrees(value) if self.angle_mode == "DEG" else value

    def functions(self):
        return {
            "sin": self._sin,
            "cos": self._cos,
            "tan": self._tan,
            "asin": self._asin,
            "acos": self._acos,
            "atan": self._atan,
            "sinh": math.sinh,
            "cosh": math.cosh,
            "tanh": math.tanh,
            "sqrt": math.sqrt,
            "cbrt": lambda x: math.cbrt(x) if hasattr(math, "cbrt") else (
                abs(x) ** (1 / 3) if x >= 0 else -abs(x) ** (1 / 3)
            ),
            "log": math.log,
            "log10": math.log10,
            "log2": math.log2,
            "exp": math.exp,
            "abs": abs,
            "fabs": math.fabs,
            "floor": math.floor,
            "ceil": math.ceil,
            "degrees": math.degrees,
            "radians": math.radians,
            "factorial": math.factorial,
        }

    def evaluate(self, expression):
        # Human-friendly replacements.
        expression = expression.replace("*", "*")
        expression = expression.replace("÷", "/")
        expression = expression.replace("−", "-")
        expression = expression.replace("^", "**")
        expression = expression.replace("π", "pi")

        # Factorial: 5! -> factorial(5), including parenthesized values.
        expression = self._replace_factorials(expression)

        tree = ast.parse(expression, mode="eval")

        functions = self.functions()

        constants = {
            "pi": math.pi,
            "e": math.e,
            "tau": math.tau,
            "inf": math.inf,
            "Ans": self.ans,
            "M": self.memory,
        }

        def ev(node):
            if isinstance(node, ast.Constant):
                if isinstance(node.value, (int, float)):
                    return node.value
                raise ValueError("Invalid value")

            if isinstance(node, ast.Name):
                if node.id in constants:
                    return constants[node.id]
                raise ValueError(f"Unknown name: {node.id}")

            if isinstance(node, ast.BinOp):
                fn = BINARY_OPS.get(type(node.op))
                if fn is None:
                    raise ValueError("Unsupported operator")
                return fn(ev(node.left), ev(node.right))

            if isinstance(node, ast.UnaryOp):
                fn = UNARY_OPS.get(type(node.op))
                if fn is None:
                    raise ValueError("Unsupported unary operator")
                return fn(ev(node.operand))

            if isinstance(node, ast.Call):
                if not isinstance(node.func, ast.Name):
                    raise ValueError("Invalid function")

                name = node.func.id
                if name not in functions:
                    raise ValueError(f"Unknown function: {name}")

                if node.keywords:
                    raise ValueError("Keyword arguments are not supported")

                args = [ev(arg) for arg in node.args]
                return functions[name](*args)

            raise ValueError("Invalid expression")

        return ev(tree.body)

    @staticmethod
    def _replace_factorials(expr):
        # Repeatedly replace simple factorial operands.
        pattern = re.compile(
            r"(\b(?:\d+(?:\.\d*)?|\.\d+|[A-Za-z_]\w*|\([^()]*\)))!"
        )

        while "!" in expr:
            new_expr = pattern.sub(r"factorial(\1)", expr)
            if new_expr == expr:
                raise ValueError("Invalid factorial expression")
            expr = new_expr

        return expr

    @staticmethod
    def format_result(value):
        if isinstance(value, int):
            return str(value)

        if isinstance(value, float):
            if math.isnan(value):
                return "nan"
            if math.isinf(value):
                return "∞" if value > 0 else "-∞"

            if value == 0:
                return "0"

            # Preserve precision while avoiding ugly trailing zeros.
            return f"{value:.15g}"

        return str(value)


# ------------------------- GUI -------------------------------

class ScientificCalculatorApp:
    BG = "#111318"
    PANEL = "#191c22"
    DISPLAY = "#0b0d10"
    TEXT = "#f3f4f6"
    MUTED = "#9ca3af"
    BUTTON = "#252a33"
    BUTTON_HOVER = "#303641"
    OPERATOR = "#343b48"
    ACCENT = "#3b82f6"
    DANGER = "#7f1d1d"

    def __init__(self, root):
        self.root = root
        self.root.title("Scientific Calculator")
        self.root.geometry("1050x720")
        self.root.minsize(850, 600)
        self.root.configure(bg=self.BG)

        self.calc = Calculator()
        self.just_calculated = False

        self.expression = tk.StringVar()
        self.result = tk.StringVar(value="0")
        self.mode_text = tk.StringVar(value="DEG")
        self.memory_text = tk.StringVar(value="M: 0")

        self._setup_styles()
        self._build_ui()
        self._bind_keyboard()

    # -------------------- UI setup ---------------------------

    def _setup_styles(self):
        style = ttk.Style()
        try:
            style.theme_use("clam")
        except tk.TclError:
            pass

        style.configure(
            "TNotebook",
            background=self.BG,
            borderwidth=0,
        )
        style.configure(
            "TNotebook.Tab",
            background=self.PANEL,
            foreground=self.MUTED,
            padding=(15, 8),
        )
        style.map(
            "TNotebook.Tab",
            background=[("selected", self.BUTTON)],
            foreground=[("selected", self.TEXT)],
        )

    def _build_ui(self):
        main = tk.Frame(self.root, bg=self.BG)
        main.pack(fill="both", expand=True, padx=12, pady=12)

        # Top display.
        display_frame = tk.Frame(main, bg=self.DISPLAY, bd=0)
        display_frame.pack(fill="x", pady=(0, 10))

        top = tk.Frame(display_frame, bg=self.DISPLAY)
        top.pack(fill="x", padx=18, pady=(12, 0))

        tk.Label(
            top,
            textvariable=self.mode_text,
            bg=self.DISPLAY,
            fg=self.ACCENT,
            font=("Arial", 11, "bold"),
        ).pack(side="left")

        tk.Label(
            top,
            textvariable=self.memory_text,
            bg=self.DISPLAY,
            fg=self.MUTED,
            font=("Arial", 10),
        ).pack(side="right")

        self.entry = tk.Entry(
            display_frame,
            textvariable=self.expression,
            bg=self.DISPLAY,
            fg=self.TEXT,
            insertbackground=self.TEXT,
            relief="flat",
            borderwidth=0,
            justify="right",
            font=("Consolas", 25),
        )
        self.entry.pack(fill="x", padx=18, pady=(5, 0), ipady=10)

        tk.Label(
            display_frame,
            textvariable=self.result,
            bg=self.DISPLAY,
            fg=self.MUTED,
            anchor="e",
            font=("Consolas", 17),
        ).pack(fill="x", padx=18, pady=(0, 12))

        # Main area.
        body = tk.Frame(main, bg=self.BG)
        body.pack(fill="both", expand=True)

        calculator_panel = tk.Frame(body, bg=self.BG)
        calculator_panel.pack(side="left", fill="both", expand=True)

        history_panel = tk.Frame(
            body,
            bg=self.PANEL,
            width=280,
        )
        history_panel.pack(side="right", fill="y", padx=(10, 0))
        history_panel.pack_propagate(False)

        self._build_buttons(calculator_panel)
        self._build_history(history_panel)

    def _build_buttons(self, parent):
        for col in range(8):
            parent.grid_columnconfigure(col, weight=1, uniform="calc")

        for row in range(8):
            parent.grid_rowconfigure(row, weight=1, uniform="calc")

        buttons = [
            # row, col, text, command, colspan
            (0, 0, "MC", self.memory_clear, 1),
            (0, 1, "MR", self.memory_recall, 1),
            (0, 2, "M+", self.memory_add, 1),
            (0, 3, "M−", self.memory_subtract, 1),
            (0, 4, "MS", self.memory_store, 1),
            (0, 5, "DEG", self.toggle_angle, 1),
            (0, 6, "(", lambda: self.insert("("), 1),
            (0, 7, ")", lambda: self.insert(")"), 1),

            (1, 0, "sin", lambda: self.function("sin"), 1),
            (1, 1, "cos", lambda: self.function("cos"), 1),
            (1, 2, "tan", lambda: self.function("tan"), 1),
            (1, 3, "log", lambda: self.function("log"), 1),
            (1, 4, "ln", lambda: self.function("ln"), 1),
            (1, 5, "√", lambda: self.function("sqrt"), 1),
            (1, 6, "x²", lambda: self.insert("^2"), 1),
            (1, 7, "xʸ", lambda: self.insert("^"), 1),

            (2, 0, "asin", lambda: self.function("asin"), 1),
            (2, 1, "acos", lambda: self.function("acos"), 1),
            (2, 2, "atan", lambda: self.function("atan"), 1),
            (2, 3, "log₂", lambda: self.function("log2"), 1),
            (2, 4, "10ˣ", lambda: self.function("10^"), 1),
            (2, 5, "eˣ", lambda: self.function("exp"), 1),
            (2, 6, "π", lambda: self.insert("pi"), 1),
            (2, 7, "e", lambda: self.insert("e"), 1),

            (3, 0, "7", lambda: self.insert("7"), 1),
            (3, 1, "8", lambda: self.insert("8"), 1),
            (3, 2, "9", lambda: self.insert("9"), 1),
            (3, 3, "÷", lambda: self.insert("/"), 1),
            (3, 4, "%", lambda: self.insert("%"), 1),
            (3, 5, "!", lambda: self.insert("!"), 1),
            (3, 6, "⌫", self.backspace, 1),
            (3, 7, "AC", self.clear, 1),

            (4, 0, "4", lambda: self.insert("4"), 1),
            (4, 1, "5", lambda: self.insert("5"), 1),
            (4, 2, "6", lambda: self.insert("6"), 1),
            (4, 3, "×", lambda: self.insert("*"), 1),
            (4, 4, "±", self.negate, 1),
            (4, 5, "Ans", lambda: self.insert("Ans"), 1),
            (4, 6, "[", lambda: self.insert("("), 1),
            (4, 7, "]", lambda: self.insert(")"), 1),

            (5, 0, "1", lambda: self.insert("1"), 1),
            (5, 1, "2", lambda: self.insert("2"), 1),
            (5, 2, "3", lambda: self.insert("3"), 1),
            (5, 3, "−", lambda: self.insert("-"), 1),
            (5, 4, "floor", lambda: self.function("floor"), 1),
            (5, 5, "ceil", lambda: self.function("ceil"), 1),
            (5, 6, "sinh", lambda: self.function("sinh"), 1),
            (5, 7, "cosh", lambda: self.function("cosh"), 1),

            (6, 0, "0", lambda: self.insert("0"), 1),
            (6, 1, ".", lambda: self.insert("."), 1),
            (6, 2, "00", lambda: self.insert("00"), 1),
            (6, 3, "+", lambda: self.insert("+"), 1),
            (6, 4, "tanh", lambda: self.function("tanh"), 1),
            (6, 5, "abs", lambda: self.function("abs"), 1),
            (6, 6, "cbrt", lambda: self.function("cbrt"), 1),
            (6, 7, "=", self.calculate, 1),

            (7, 0, "7.5e3", lambda: self.insert("7.5e3"), 2),
            (7, 2, "10ˣ", lambda: self.insert("10^"), 1),
            (7, 3, "mod", lambda: self.insert("%"), 1),
            (7, 4, "radians", lambda: self.function("radians"), 1),
            (7, 5, "degrees", lambda: self.function("degrees"), 1),
            (7, 6, "HIST", self.focus_history, 2),
        ]

        for row, col, text, command, colspan in buttons:
            bg = self.BUTTON

            if text in {"÷", "×", "−", "+", "=", "^", "xʸ", "%", "mod"}:
                bg = self.OPERATOR
            elif text == "=":
                bg = self.ACCENT
            elif text in {"AC", "⌫"}:
                bg = self.DANGER

            button = tk.Button(
                parent,
                text=text,
                command=command,
                bg=bg,
                fg=self.TEXT,
                activebackground=self.BUTTON_HOVER,
                activeforeground=self.TEXT,
                relief="flat",
                bd=0,
                font=("Arial", 11, "bold"),
                cursor="hand2",
            )
            button.grid(
                row=row,
                column=col,
                columnspan=colspan,
                sticky="nsew",
                padx=3,
                pady=3,
            )

            button.bind(
                "<Enter>",
                lambda e, b=button: b.configure(bg=self.BUTTON_HOVER),
            )
            button.bind(
                "<Leave>",
                lambda e, b=button, color=bg: b.configure(bg=color),
            )

    def _build_history(self, parent):
        header = tk.Frame(parent, bg=self.PANEL)
        header.pack(fill="x", padx=12, pady=(12, 5))

        tk.Label(
            header,
            text="History",
            bg=self.PANEL,
            fg=self.TEXT,
            font=("Arial", 13, "bold"),
        ).pack(side="left")

        tk.Button(
            header,
            text="Clear",
            command=self.clear_history,
            bg=self.BUTTON,
            fg=self.TEXT,
            activebackground=self.BUTTON_HOVER,
            relief="flat",
            bd=0,
        ).pack(side="right")

        frame = tk.Frame(parent, bg=self.PANEL)
        frame.pack(fill="both", expand=True, padx=10, pady=(0, 10))

        scrollbar = tk.Scrollbar(frame)
        scrollbar.pack(side="right", fill="y")

        self.history_list = tk.Listbox(
            frame,
            bg=self.PANEL,
            fg=self.TEXT,
            selectbackground=self.ACCENT,
            selectforeground="white",
            activestyle="none",
            relief="flat",
            borderwidth=0,
            font=("Consolas", 10),
            yscrollcommand=scrollbar.set,
        )
        self.history_list.pack(side="left", fill="both", expand=True)

        scrollbar.configure(command=self.history_list.yview)

        self.history_list.bind("<Double-Button-1>", self.use_history)

    # -------------------- Input handling ---------------------

    def insert(self, text):
        if self.just_calculated:
            self.expression.set("")
            self.just_calculated = False

        self.entry.insert(tk.INSERT, text)
        self.entry.focus_set()

    def function(self, name):
        if name == "ln":
            name = "log"

        if name == "10^":
            self.insert("10^(")
        elif name == "exp":
            self.insert("exp(")
        else:
            self.insert(f"{name}(")

    def backspace(self):
        try:
            pos = self.entry.index(tk.INSERT)
            if pos > 0:
                self.entry.delete(pos - 1, pos)
        except tk.TclError:
            pass

    def clear(self):
        self.expression.set("")
        self.result.set("0")
        self.just_calculated = False
        self.entry.focus_set()

    def negate(self):
        text = self.expression.get()
        if not text:
            self.insert("-")
        else:
            self.expression.set(f"-({text})")

    # -------------------- Calculation ------------------------

    def calculate(self, event=None):
        expression = self.expression.get().strip()

        if not expression:
            return

        try:
            value = self.calc.evaluate(expression)
            formatted = self.calc.format_result(value)

            self.calc.ans = value
            self.result.set(formatted)

            history_item = f"{expression} = {formatted}"
            self.calc.history.append((expression, formatted))
            self.history_list.insert(tk.END, history_item)
            self.history_list.see(tk.END)

            self.just_calculated = True

        except ZeroDivisionError:
            self.result.set("Error: division by zero")
            self.just_calculated = False

        except (ValueError, SyntaxError, TypeError, OverflowError) as error:
            self.result.set(f"Error: {error}")
            self.just_calculated = False

        except Exception as error:
            self.result.set(f"Error: {error}")
            self.just_calculated = False

    # -------------------- Angle mode -------------------------

    def toggle_angle(self):
        self.calc.angle_mode = (
            "RAD" if self.calc.angle_mode == "DEG" else "DEG"
        )
        self.mode_text.set(self.calc.angle_mode)

    # -------------------- Memory -----------------------------

    def _current_value(self):
        text = self.expression.get().strip()

        if text:
            return self.calc.evaluate(text)

        return self.calc.ans

    def memory_clear(self):
        self.calc.memory = 0
        self.update_memory_display()

    def memory_recall(self):
        value = self.calc.format_result(self.calc.memory)
        self.insert(value)

    def memory_add(self):
        try:
            self.calc.memory += self._current_value()
            self.update_memory_display()
        except Exception:
            self.result.set("Error: invalid memory value")

    def memory_subtract(self):
        try:
            self.calc.memory -= self._current_value()
            self.update_memory_display()
        except Exception:
            self.result.set("Error: invalid memory value")

    def memory_store(self):
        try:
            self.calc.memory = self._current_value()
            self.update_memory_display()
        except Exception:
            self.result.set("Error: invalid memory value")

    def update_memory_display(self):
        self.memory_text.set(
            f"M: {self.calc.format_result(self.calc.memory)}"
        )

    # -------------------- History ----------------------------

    def clear_history(self):
        self.calc.history.clear()
        self.history_list.delete(0, tk.END)

    def use_history(self, event=None):
        selection = self.history_list.curselection()
        if not selection:
            return

        index = selection[0]
        expression, _ = self.calc.history[index]

        self.expression.set(expression)
        self.entry.focus_set()
        self.entry.icursor(tk.END)

    def focus_history(self):
        self.history_list.focus_set()

    # -------------------- Keyboard ----------------------------

    def _bind_keyboard(self):
        self.root.bind("<Return>", self.calculate)
        self.root.bind("<KP_Enter>", self.calculate)
        self.root.bind("<Escape>", lambda e: self.clear())
        self.root.bind("<Control-l>", lambda e: self.clear())
        self.root.bind("<Control-h>", lambda e: self.focus_history())

        # Keyboard-friendly shortcuts.
        self.root.bind("<F1>", lambda e: self.toggle_angle())

        # Prevent accidental characters that aren't valid expression input
        # only when they are typed through the calculator entry. The actual
        # expression engine remains unrestricted with respect to number size.
        self.entry.bind("<KeyPress>", self._keyboard_filter)

    def _keyboard_filter(self, event):
        allowed = (
            "0123456789"
            "abcdefghijklmnopqrstuvwxyz"
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            ".+-*/%^_()!"
        )

        # Allow navigation, deletion, paste, copy, etc.
        if event.keysym in {
            "BackSpace", "Delete", "Left", "Right", "Home", "End",
            "Up", "Down", "Return", "KP_Enter", "Tab",
        }:
            return

        if event.state & 0x4:  # Ctrl
            return

        if event.char and event.char in allowed:
            return

        # Let Tk handle IME/system keys.
        if not event.char:
            return

        return "break"


def main():
    root = tk.Tk()
    app = ScientificCalculatorApp(root)
    app.entry.focus_set()
    root.mainloop()


if __name__ == "__main__":
    main()

