
import tkinter as tk
import networkx as nx
import matplotlib.pyplot as plt


class GraphEditor:
    def __init__(self, root):
        self.root = root
        self.root.title("Community Detection")

        self.canvas = tk.Canvas(root, width=800, height=600, bg="white")
        self.canvas.pack()

        self.graph = nx.Graph()
        self.nodes = {}
        self.node_radius = 20

        self.selected_node = None
        self.node_counter = 0

        # Buttons
        button_frame = tk.Frame(root)
        button_frame.pack(pady=10)

        tk.Button(
            button_frame,
            text="Detect Communities",
            command=self.detect_communities
        ).pack(side=tk.LEFT, padx=5)

        tk.Button(
            button_frame,
            text="Clear Graph",
            command=self.clear_graph
        ).pack(side=tk.LEFT, padx=5)

        tk.Label(
            root,
            text="Click an empty area to create a node. "
                 "Click two nodes to connect them."
        ).pack()

        self.canvas.bind("<Button-1>", self.canvas_click)

    def canvas_click(self, event):
        """Handle mouse clicks on the canvas."""

        clicked_node = self.find_node(event.x, event.y)

        if clicked_node is None:
            # Create a new node
            self.create_node(event.x, event.y)

        else:
            # Select node / create edge
            self.select_node(clicked_node)

    def create_node(self, x, y):
        """Create a node at the clicked position."""

        self.node_counter += 1
        node_name = str(self.node_counter)

        self.graph.add_node(node_name)

        circle = self.canvas.create_oval(
            x - self.node_radius,
            y - self.node_radius,
            x + self.node_radius,
            y + self.node_radius,
            fill="lightblue",
            outline="black",
            width=2
        )

        label = self.canvas.create_text(
            x,
            y,
            text=node_name,
            font=("Arial", 12, "bold")
        )

        self.nodes[node_name] = {
            "x": x,
            "y": y,
            "circle": circle,
            "label": label
        }

    def find_node(self, x, y):
        """Return the node closest to the clicked position."""

        for node, data in self.nodes.items():
            distance = (
                (x - data["x"]) ** 2 +
                (y - data["y"]) ** 2
            ) ** 0.5

            if distance <= self.node_radius:
                return node

        return None

    def select_node(self, node):
        """Select a node or create an edge."""

        if self.selected_node is None:
            self.selected_node = node

            self.canvas.itemconfig(
                self.nodes[node]["circle"],
                fill="orange"
            )

        else:
            # Clicking the same node cancels selection
            if node == self.selected_node:
                self.canvas.itemconfig(
                    self.nodes[node]["circle"],
                    fill="lightblue"
                )
                self.selected_node = None
                return

            # Add an edge
            if not self.graph.has_edge(self.selected_node, node):
                self.graph.add_edge(
                    self.selected_node,
                    node
                )

                self.draw_edge(
                    self.selected_node,
                    node
                )

            # Reset selection
            self.canvas.itemconfig(
                self.nodes[self.selected_node]["circle"],
                fill="lightblue"
            )

            self.selected_node = None

    def draw_edge(self, node1, node2):
        """Draw an edge between two nodes."""

        x1 = self.nodes[node1]["x"]
        y1 = self.nodes[node1]["y"]

        x2 = self.nodes[node2]["x"]
        y2 = self.nodes[node2]["y"]

        self.canvas.create_line(
            x1,
            y1,
            x2,
            y2,
            fill="black",
            width=2
        )

        # Move nodes/labels to the front
        self.canvas.tag_raise(
            self.nodes[node1]["circle"]
        )
        self.canvas.tag_raise(
            self.nodes[node1]["label"]
        )

        self.canvas.tag_raise(
            self.nodes[node2]["circle"]
        )
        self.canvas.tag_raise(
            self.nodes[node2]["label"]
        )

    def detect_communities(self):
        """Run the Louvain community detection algorithm."""

        if len(self.graph.nodes) == 0:
            return

        if len(self.graph.edges) == 0:
            print("Add some edges first.")
            return

        communities = nx.community.louvain_communities(
            self.graph,
            seed=42
        )

        print("\nDetected Communities:")

        for i, community in enumerate(communities, start=1):
            print(
                f"Community {i}: "
                f"{sorted(community, key=int)}"
            )

        # Create a mapping from node -> community
        community_map = {}

        for i, community in enumerate(communities):
            for node in community:
                community_map[node] = i

        # Show the graph using NetworkX
        plt.figure(figsize=(8, 6))

        pos = nx.spring_layout(
            self.graph,
            seed=42
        )

        nx.draw(
            self.graph,
            pos,
            with_labels=True,
            node_color=[
                community_map[node]
                for node in self.graph.nodes
            ],
            node_size=1000,
            cmap=plt.cm.Set3,
            font_weight="bold"
        )

        plt.title("Detected Communities")
        plt.show()

    def clear_graph(self):
        """Delete the entire graph."""

        self.graph.clear()
        self.nodes.clear()
        self.node_counter = 0
        self.selected_node = None

        self.canvas.delete("all")


# -------------------------------------------------
# Main program
# -------------------------------------------------

root = tk.Tk()

app = GraphEditor(root)

root.mainloop()


