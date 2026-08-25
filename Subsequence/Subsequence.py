def longest_increasing_subsequence(X):
    n = len(X)

    if n == 0:
        return []

    # dp[i] = length of the longest increasing subsequence
    # ending at index i
    dp = [1] * n

    # prev[i] = previous index in the subsequence
    prev = [-1] * n

    for i in range(n):
        for j in range(i):
            if X[j] < X[i] and dp[j] + 1 > dp[i]:
                dp[i] = dp[j] + 1
                prev[i] = j

    # Find where the longest subsequence ends
    last = max(range(n), key=lambda i: dp[i])

    # Reconstruct the subsequence
    result = []
    while last != -1:
        result.append(X[last])
        last = prev[last]

    return result[::-1]


# Example
X = [10, 22, 9, 33, 21, 50, 41, 60]

print(longest_increasing_subsequence(X))
