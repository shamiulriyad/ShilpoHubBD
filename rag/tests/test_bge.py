from sentence_transformers import SentenceTransformer

print("Loading BGE-M3...")

model = SentenceTransformer("BAAI/bge-m3")

print("Model loaded successfully!")

texts = [
    "What is a noun?",
    "A noun is a word that names a person, place, thing, or idea."
]

embeddings = model.encode(
    texts,
    normalize_embeddings=True
)

print("Embedding shape:", embeddings.shape)
print("Embedding dimension:", embeddings.shape[1])
