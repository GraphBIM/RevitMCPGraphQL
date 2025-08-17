import requests
import json

class MCPClient:
    def __init__(self, url="http://localhost:5000/graphql"):
        self.url = url

    def run_query(self, query):
        response = requests.post(self.url, json={'query': query})
        try:
            return response.json()
        except Exception as e:
            return {"error": str(e), "raw": response.text}

    def health(self):
        query = """
        { health }
        """
        return self.run_query(query)

    def categories(self):
        query = """
        { categories { id name } }
        """
        return self.run_query(query)

    def elements(self, category_name=None):
        if category_name:
            query = f"""
            {{ elements(categoryName: \"{category_name}\") {{ id name }} }}
            """
        else:
            query = """
            { elements { id name } }
            """
        return self.run_query(query)

    def rooms(self):
        query = """
        { rooms { id name number area } }
        """
        return self.run_query(query)

if __name__ == "__main__":
    client = MCPClient()
    print("Health:", json.dumps(client.health(), indent=2))
    print("Categories:", json.dumps(client.categories(), indent=2))
    print("Elements (Walls):", json.dumps(client.elements("Walls"), indent=2))
    print("Rooms:", json.dumps(client.rooms(), indent=2))
