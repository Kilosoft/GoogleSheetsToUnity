import sys
import persons
import items



def main():
    pathJson = sys.argv[1] if len(sys.argv) > 1 else ""
    pathScripts = sys.argv[2] if len(sys.argv) > 2 else ""
        
    tables = [persons, items]
    for table in tables:
        table.main(pathJson, pathScripts)


if __name__ == "__main__":
    main()
