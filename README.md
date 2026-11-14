# VR Garment Showroom

A Unity virtual showroom project for exploring garments in a 3D environment. The project combines Member 1's showroom scene, Member 2's player movement and product interaction, and Member 3's catalogue, navigation, and product information scripts.

## Open the project

1. Open this repository through Unity Hub using the project version listed in `ProjectSettings/ProjectVersion.txt`.
2. Open `Assets/Member1/Scenes/ThreadHaven_Showroom.unity` to view the showroom environment.
3. Review [Member 3's setup and usage guide](Assets/Member3/README.md) before connecting the player and UI in the Unity Editor.

The build scene list currently includes the showroom. The menu scene is a Canvas scaffold; add its visual controls and place it before the showroom in Build Profiles if the application should start from the menu.

## Member 3 documentation

- [Implementation, setup, and usage](Assets/Member3/README.md)
- [Product data and catalogue](Assets/Member3/Products/README.md)
- [Showroom scene integration](Assets/Member3/UI/ShowroomScene_Integration.md)
- [UI refinements](Assets/Member3/UI/Commit19_UI_Refinements.md)
- [Layout reference visuals](Assets/Member3/Documentation/Screenshots/README.md)
- [PostgreSQL and Prisma API setup](backend/README.md)

## Product database

The catalogue can load from the local PostgreSQL-backed API. Prisma maps the product records and categories to PostgreSQL; Unity requests catalogue JSON from the API. The existing sample JSON remains available as a fallback when the API is not running. See [backend setup](backend/README.md).

## Project scope

The project demonstrates a software-based VR showroom concept for the garment industry. Product records in the sample catalogue are illustrative examples, not specifications for real retail products.
