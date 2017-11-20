class Element(object):    
    def __init__(self, kind, left, right, R, E, id):
        self.R = R
        self.left = left
        self.right = right
        self.kind = kind
        self.state = True
        self.id = id
        self.E = E
        self.I = 0
        self.U = 0
        self.critP = 0

    def near(self, v):
        return self.left if self.right == v else self.right

    def __eq__(self, other):
        return self.id == other.id

    def __repr__(self):
        return '{' + str(self.left) + ', ' +\
                     str(self.right) + ', ' +\
                     'R: ' + str(self.R) + ', ' +\
                     'I: ' + str(self.I) + ', ' +\
                     'U: ' + str(self.U) + ', ' +\
                     'E: ' + str(self.E) + '}'
