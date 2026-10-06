import { defineComponent } from "vue";

export default defineComponent({
  name: "ProductComponent",

  data() {
    return {
      product: {
        name: "Barrette RAM DDR5 16 Go",
        reference: "RAM-DDR5-16GB",

        description:
          "Barrette de mémoire vive DDR5 de 16 Go destinée aux ordinateurs de bureau et aux stations de travail.",

        capacity: "16 Go",

        productionTime: 8,

        quantity: 500,

        status: "Disponible",

        recipe: [
          {
            id: 1,
            name: "Préparation du PCB",
            description:
              "Préparation et contrôle du circuit imprimé destiné à recevoir les composants mémoire."
          },

          {
            id: 2,
            name: "Placement des composants",
            description:
              "Placement automatique des puces mémoire et des composants électroniques sur le PCB."
          },

          {
            id: 3,
            name: "Soudage",
            description:
              "Passage dans le four de refusion afin de souder les composants sur le circuit imprimé."
          },

          {
            id: 4,
            name: "Contrôle optique",
            description:
              "Inspection automatique du placement des composants et de la qualité des soudures."
          },

          {
            id: 5,
            name: "Test fonctionnel",
            description:
              "Test de la barrette afin de vérifier sa capacité, sa fréquence et son bon fonctionnement."
          },

          {
            id: 6,
            name: "Étiquetage et conditionnement",
            description:
              "Identification de la barrette puis conditionnement du produit fini."
          }
        ],

        machines: [
          {
            id: 1,
            name: "Machine de placement CMS",
            description:
              "Placement automatique des composants électroniques sur le PCB."
          },

          {
            id: 2,
            name: "Four de refusion",
            description:
              "Soudage des composants électroniques sur le circuit imprimé."
          },

          {
            id: 3,
            name: "Machine AOI",
            description:
              "Inspection optique automatique du circuit imprimé."
          },

          {
            id: 4,
            name: "Banc de test mémoire",
            description:
              "Vérification du fonctionnement et des performances de la barrette."
          },

          {
            id: 5,
            name: "Poste de conditionnement",
            description:
              "Étiquetage et emballage du produit fini."
          }
        ]
      }
    };
  },

  methods: {
    selectProduct() {
      console.log("Produit sélectionné :", this.product.name);
    }
  }
});