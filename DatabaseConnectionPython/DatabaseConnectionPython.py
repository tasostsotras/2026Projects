import sqlite3

# Connect to the database
conn = sqlite3.connect('vasi.db')

# Create a cursor object
cursor = conn.cursor()

# Execute a query
cursor.execute("SELECT * FROM xristes")

# Fetch all rows
rows = cursor.fetchall()

# Print the data
for row in rows:
    print(row)

# Close the cursor and the connection
cursor.close()
conn.close()
