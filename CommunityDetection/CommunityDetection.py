import networkx as nx
import matplotlib.pyplot as plt

# Create a graph
G = nx.Graph()

# Add edges
edges = [
    ("A", "B"), ("A", "C"), ("B", "C"),
    ("B", "D"), ("C", "D"),
    ("E", "F"), ("E", "G"), ("F", "G"),
    ("D", "E")
]

G.add_edges_from(edges)

# Detect communities using Louvain
communities = nx.community.louvain_communities(G, seed=42)

# Display communities
print("Detected communities:")
for i, community in enumerate(communities, start=1):
    print(f"Community {i}: {sorted(community)}")

# Assign a community number to each node
community_map = {}
for i, community in enumerate(communities):
    for node in community:
        community_map[node] = i

# Draw the graph
pos = nx.spring_layout(G, seed=42)

nx.draw(
    G,
    pos,
    with_labels=True,
    node_color=[community_map[node] for node in G.nodes()],
    node_size=1000,
    cmap=plt.cm.Set3
)

plt.title("Community Detection using Louvain Algorithm")
plt.show()
