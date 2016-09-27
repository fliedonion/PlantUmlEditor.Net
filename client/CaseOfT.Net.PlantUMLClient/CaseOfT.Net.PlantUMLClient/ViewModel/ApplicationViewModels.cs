using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.ViewModel {
    class ApplicationViewModels {

        private static Presenter editorPagePresenter = null;

        public static Presenter EditorPagePresenter {
            get {
                if (editorPagePresenter == null) {
                    editorPagePresenter = new Presenter();
                }
                return editorPagePresenter;
            }
        }

        private static LibLocations libraryLocationModel = null;
        public static LibLocations LibraryLocationModel {
            get {
                if (libraryLocationModel == null) {
                    libraryLocationModel = new LibLocations();
                }
                return libraryLocationModel;
            }
        }


    }
}
