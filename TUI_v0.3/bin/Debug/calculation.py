from collections import defaultdict
from Gauss import Gauss
def calculate(ids, adj, V):
    '''
        V - number of points
        ids - dict ID : Element (list of edges)
        adj - defaultdict point: list of Elements
    '''
    Adj = {}
    n = 0
    for v in adj:
        Adj[v] = []
        for i in range (len(adj[v])):
                ind, R, E, on = adj[v][i][0], adj[v][i][1], adj[v][i][2], adj[v][i][3]
                if on:
                    Adj[v].append([ind, R, E])
                    if ind > n:
                        n = ind
    graph = defaultdict(list)
    # n = len(Adj)
    mas = [i for i in range(n+1)]
    for v in Adj:
        zeros = []
        zeros_ind = []
        for i in range(len(Adj[v])):
            if Adj[v][i][1] == 0:
                zeros.append(i)
                zeros_ind.append(Adj[v][i][0])
        i = 0
        zeros.reverse()
        for i in zeros:
            del Adj[v][i]
        
        for u in range(len(zeros_ind)):
            if mas[zeros_ind[u]] == zeros_ind[u]:
                mas[zeros_ind[u]] = mas[v]
            else:
                mas[v] = mas[zeros_ind[u]]
    for v in Adj:
        for to in Adj[v]:
            to[0] = mas[to[0]]
            if(to[0] == mas[v]):
                continue
            to[1] = 1/to[1]
            a = mas[v]
            graph[a].append(to)
    matrix = []
    for i in range(n+2):
        matrix.append([])
        for j in range(n+2):
            matrix[i].append(0)
    for v in graph:
        E = 0
        R = 0
        for to in graph[v]:
            u = to[0]
            matrix[v-1][u-1] -= to[1]
            R += to[1]
            E += to[1]*to[2]
        matrix[v-1][v-1] = R
        matrix[v-1][n+1] = E
    answer = [0 for i in range(n+1)]
    Gauss(matrix, answer)
    for i in ids:
        a = mas[ids[i].left]
        b = mas[ids[i].right]
        u1 = answer[a-1]
        u2 = answer[b-1]
        ids[i].I = ids[i].U = 0
        ids[i].U = abs(u1-u2)
        if ids[i].R > 0:
            ids[i].I = round(ids[i].U/ids[i].R, 5)
        if ids[i].kind == "rEMF" or ids[i].kind == "lEMF":
           ids[i].I = 0
        ids[i].U = round(ids[i].U, 5);

    return(ids)
    pass
