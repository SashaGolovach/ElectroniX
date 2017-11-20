from __future__ import division
from collections import deque, defaultdict

def resistance(ids, st, fin, U, V):

    # converts the list of edges into adjacency list

    graph = [defaultdict(list) for _ in range(V)]
    ind = {}
    counter = 0

    for id, e in ids.items():
        v = e.left
        u = e.right
        if v not in ind:
            ind[v] = counter
            counter += 1
        if u not in ind:
            ind[u] = counter
            counter += 1
        graph[ind[v]][ind[u]].append(e.R)
        graph[ind[u]][ind[v]].append(e.R)
            
    start = ind[st]
    end = ind[fin]
    
    # eliminating zero-edges

    for u, adj in enumerate(graph):
        zeros = [v for v in adj if 0 in adj[v]]
        for z in zeros:
            if z in (start, end): continue
            for v, weights in graph[z].items():
                if u != v:
                    graph[u][v].extend(weights)
                    graph[v][u].extend(weights)
                graph[v][z] = []
            graph[z] = defaultdict(list)

    if 0 in graph[start][end]:
        return 0.0

    # checking connectedness

    used = [False] * V
    queue = deque([start])

    while queue:
        u = queue.popleft()
        used[u] = True
        for v, weights in graph[u].items():
            if not used[v] and weights:
                queue.append(v)

    if not used[end]:
        return float('inf')
            
    # eliminating multiedges

    for adj in graph:
        for weights in adj.values():
            if len(weights) < 2: continue
            total = 1/sum(1/w for w in weights)
            weights[:] = [total]
    # relabeling vertices 

    new = {}
    rev = {}
    ind = 0
    for u, is_used in enumerate(used):
        if is_used:
            new[ind] = u
            rev[u] = ind
            ind += 1

    # constructing Laplacian matrix

    V = sum(used)
    L = [[0]*2*V for i in range(V)]
    start, end = rev[start], rev[end]

    for i in range(V):
        adj = graph[new[i]]
        L[i][V+i] = 1
        for j in range(V):
            if i == j:
                L[i][i] = sum(1/w[0] for w in adj.values() if w) + 1/V
            else:
                L[i][j] = (-1/adj[new[j]][0] if adj[new[j]] else 0) + 1/V

    # finding the resistance
    
    for k in range(V):
        mul = L[k][k]
        for i in range(k, 2*V):
            L[k][i] /= mul
        for i in range(k+1, V):
            mul = L[i][k]/L[k][k]
            for j in range(k, 2*V):
                L[i][j] -= mul*L[k][j]
                
    for k in range(V-1, -1, -1):
        for i in range(k):
            mul = L[i][k]
            for j in range(k, 2*V):
                L[i][j] -= mul*L[k][j]
                
    Lplus = [[a - 1/V for a in row[V:]] for row in L]
    R = Lplus[start][start] + Lplus[end][end] - 2*Lplus[start][end]
##    I = U/R
##    eab = [0]*V
##    eab[start] = I
##    eab[end] = -I
##    Phi = [0] * V
##    
##    for i in range(V):
##        for j in range(V):
##            Phi[i] += eab[j]*Lplus[i][j]
##
##    for id, e in ids.items():
##        i = ind[e.left]
##        j = ind[e.right]
##        e.I = (Phi[i] - Phi[j])/e.R
    
    return R
