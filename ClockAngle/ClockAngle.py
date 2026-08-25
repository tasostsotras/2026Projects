while True:
    try:
        hours = int(input("Enter hours (1-12): "))

        if 1 <= hours <= 12:
            break

        print("Invalid hours. Please enter a value from 1 to 12.")

    except ValueError:
        print("Invalid input. Please enter a whole number.")


while True:
    try:
        minutes = int(input("Enter minutes (0-59): "))

        if 0 <= minutes <= 59:
            break

        print("Invalid minutes. Please enter a value from 0 to 59.")

    except ValueError:
        print("Invalid input. Please enter a whole number.")


# Calculate the positions of the hands
hour_angle = (hours % 12) * 30 + minutes * 0.5
minute_angle = minutes * 6

# Calculate the smaller angle
angle = abs(hour_angle - minute_angle)
angle = min(angle, 360 - angle)

print(f"The angle between the hands is {angle} degrees.")