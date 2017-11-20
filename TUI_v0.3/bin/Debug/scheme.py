from element import Element
from calculation import calculate
from collections import defaultdict
from pickle import load, dump
from copy import deepcopy

class Scheme(object):
    
    def __init__(self, ids=None, wires=None):
        '''
        V - number of points
        ids - dict ID : Element (list of edges)
        adj - defaultdict point: list of Elements
        counter - is used for id assignment
        '''
        self.counter = max(ids) if ids else 0
        self.ids = ids or {} # already existing or empty
        self.wires = wires or {}
        self.V = 0

    def points(self, id1, pos1, id2, pos2):
        '''returns two points, given two elements with id1 and id2 and
           positions of points relatively to the elements'''
        e1 = self.ids[id1]
        e2 = self.ids[id2]
        first = e1.left if pos1 == 'l' else e1.right
        second = e2.left if pos2 == 'l' else e2.right
        return [first, second]

    def updateI(self):
        '''calculate the current on every element'''
        adj = defaultdict(list)
        for e in self.ids.values():
            for l in self.wires.values():
                if e.left in l:
                    for c in l:                                     
                        if c != e.right and c != e.left:
                            adj[e.left].append([c, 0, 0, 1])
                    break
            adj[e.left].append([e.right, e.R, e.E, e.state])
            for l in self.wires.values():
                if e.right in l:
                    for c in l:
                        if c != e.left and c != e.right:
                            adj[e.right].append([c, 0, 0, 1])
                    break
            adj[e.right].append([e.left, e.R, -e.E, e.state])

            
        f = open("scheme.txt", "w")
        f.write(str(adj))
        calculate(self.ids, adj, self.V)
        cop = deepcopy(self)
        for id, e in self.ids.items():
            if e.kind == 'lamp' and e.U*e.I > e.critP:
                r = cop.ids.pop(id)
                for v, el in adj.items():
                    if [r.near(v), r.R, r.E, r.state] in el:
                        el.remove([r.near(v), r.R, r.E, r.state])
        calculate(cop.ids, adj, cop.V)
        for id, e in self.ids.items():
            if e.kind == 'lamp' and e.U*e.I > e.critP:
                e.U = e.I = 0
                e.state = 0
            else:
                self.ids[id].I = cop.ids[id].I
                self.ids[id].U = cop.ids[id].U
        f.write(str(adj))
        f.close()

    def addElement(self, kind, e=None):
        '''adds a new element'''
        self.counter += 1
        if e:
            self.ids[self.counter] = e
        else:
            if kind == 'resistor' or kind == 'ammeter' or kind == 'voltmeter':
                e = Element(kind, self.V+1, self.V+2, 1.0, 0, self.counter)
            elif kind == 'lEMF':
                e = Element(kind, self.V+1, self.V+2, 1.0, 1.0, self.counter)
            elif kind == 'rEMF':
                e = Element(kind, self.V+1, self.V+2, 1.0, -1.0, self.counter)
            elif kind == 'lamp':
                e = Element(kind, self.V+1, self.V+2, 1.0, 0.0, self.counter)
            elif kind == 'switch':
                e = Element(kind, self.V+1, self.V+2, 0.00001, 0.0, self.counter)
            self.ids[self.counter] = e
            self.wires[self.V+1] = {self.V+1}
            self.wires[self.V+2] = {self.V+2}
            self.V += 2
        return e.id

    def removePoint(self, id, pos):
        if pos == 'l':
            p = self.ids[id].left
        else:
            p = self.ids[id].right
        for l in self.wires.values():
            if p in l:
                l.remove(p)
                break
        self.wires[self.V+1] = {p}
        self.V += 1

    def removeElement(self, id):
        '''removes an element'''
        e = self.ids[id]
        for l in self.wires.values():
            if e.left in l:
                l.remove(e.left)
                break
        for l in self.wires.values():
            if e.right in l:
                l.remove(e.right)
                break
        self.ids.pop(id)
        
    def connectElement(self, id1, pos1, id2, pos2):
        '''connects two points of two resistors and
           returns the id of a new wire'''
        first, second = self.points(id1, pos1, id2, pos2)
        a = b = 0
        A = B = set()
        for i, l in self.wires.items():
            if first in l:
                A = l
                a = i
                break
        for i, l in self.wires.items():
            if second in l:
                B = l
                b = i
                break
        self.wires[a] = A | B
        del self.wires[b]

    def setProperty(self, id, name, val):
        '''sets property with name 'name'
           ('R' - resistence, 'E' - EMF)
           of an element to value 'val' '''
        if val <= 0 and name != 'state':
            raise Exception("Do not enter values less or equal to zero")
        e = self.ids[id]
        if name == 'R':
            e.R = val                
        elif name == 'E':
            e.E = val
        elif name == 'critP':
            e.critP = val
        elif name == 'I':
            e.I = val
        elif name == 'U':
            e.U = val
        elif name == 'state':
            e.state = val

    def save(self, filename):
        with open(filename, 'wb') as f:
            dump(self, f)
            
    def load(self, filename):
        with open(filename, 'rb') as f:
            return load(f)

    def cop(self):
        return deepcopy(self)

s = Scheme()

