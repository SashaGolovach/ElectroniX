EPS = 1e-10
def Gauss(matrix, answer):
    n = len(matrix)
    m = len(matrix[0]) - 1
    where = [-1 for i in range(m+1)]
    col = row = 0
    while(col < m and row < n):
        cur = row
        for i in range(row,n):
            if abs(matrix[i][col]) > abs(matrix[cur][col]):
                cur = i
        if abs(matrix[cur][col]) < EPS:
            col += 1
            continue
        for i in range(col, m+1):
            matrix[row][i], matrix[cur][i] = matrix[cur][i], matrix[row][i]
        where[col] = row
        for i in range(n):
           if(i != row):
               c = matrix[i][col]/matrix[row][col]
               for j in range(col, m+1):
                   matrix[i][j] -= matrix[row][j] * c;
        row += 1
        col += 1
    for i in range(m):
        if(where[i] != -1):
            answer[i] = matrix[where[i]][m] / matrix[where[i]][i]
   
   
